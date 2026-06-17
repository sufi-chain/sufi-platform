using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.AIManagement.Configuration;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.AIManagement.Data;

/// <summary>
/// Seeds the AIManagement file structure when file-manager module is configured.
/// This contributor is automatically discovered and executed by ABP's data seeding system.
/// </summary>
public class AIManagementFileStructureDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IFileStructureRepository _fileStructureRepository;
    private readonly IFileFolderRepository _fileFolderRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly AIManagementOptions _aiOptions;
    private readonly FileManagerOptions _fileManagerOptions;
    private readonly ILogger<AIManagementFileStructureDataSeedContributor> _logger;

    public AIManagementFileStructureDataSeedContributor(
        IFileStructureRepository fileStructureRepository,
        IFileFolderRepository fileFolderRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IOptions<AIManagementOptions> aiOptions,
        IOptions<FileManagerOptions> fileManagerOptions,
        ILogger<AIManagementFileStructureDataSeedContributor> logger)
    {
        _fileStructureRepository = fileStructureRepository;
        _fileFolderRepository = fileFolderRepository;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _aiOptions = aiOptions.Value;
        _fileManagerOptions = fileManagerOptions.Value;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            if (!_aiOptions.SeedFileStructure)
            {
                _logger.LogDebug(
                    "AIManagement file structure seeding is disabled. TenantId={TenantId}",
                    context?.TenantId);
                return;
            }

            _logger.LogInformation(
                "AIManagement file structure seeding started. TenantId={TenantId}",
                context?.TenantId);

            // Add AIManagement structure to FileManagerOptions
            _aiOptions.AddDefaultFileStructure(_fileManagerOptions);

            // Find the AIManagement structure config
            var config = _fileManagerOptions.Structures
                .Find(s => s.Key == AIManagementFileStructureKeys.AIManagement);

            if (config == null)
            {
                _logger.LogWarning(
                    "AIManagement file structure configuration not found after adding defaults.");
                return;
            }

            await SeedStructureAsync(config);

            _logger.LogInformation("AIManagement file structure seeding completed.");
        }
    }

    private async Task SeedStructureAsync(FileStructureConfig config)
    {
        var existing = await _fileStructureRepository.FindByKeyAsync(config.Key);

        if (existing != null)
        {
            await EnsureStructureRootFolderAsync(existing);
            _logger.LogDebug(
                "File structure '{StructureKey}' already exists with ID {Id}.",
                config.Key,
                existing.Id);
            return;
        }

        var entity = new FileStructure(
            _guidGenerator.Create(),
            config.Key,
            config.DisplayName,
            config.AllowedFileTypes,
            config.AllowedExtensions,
            config.AllowedMimeTypes,
            config.MaxFileSize)
        {
            Description = config.Description,
            MinImageWidth = config.MinImageWidth,
            MinImageHeight = config.MinImageHeight,
            MaxImageWidth = config.MaxImageWidth,
            MaxImageHeight = config.MaxImageHeight,
            IsMultiple = config.IsMultiple,
            MaxCount = config.MaxCount,
            IsRequired = config.IsRequired,
            GenerateThumbnail = config.GenerateThumbnail,
            ThumbnailWidth = config.ThumbnailWidth,
            ThumbnailHeight = config.ThumbnailHeight,
            EnableWebPConversion = config.EnableWebPConversion,
            WebPQuality = config.WebPQuality,
            StorageProvider = config.StorageProvider,
            IsPublicAccess = config.IsPublicAccess,
            BaseUrl = config.BaseUrl,
            ResizeLargeImages = config.ResizeLargeImages
        };

        await _fileStructureRepository.InsertAsync(entity, autoSave: true);
        await EnsureStructureRootFolderAsync(entity);
        
        _logger.LogInformation(
            "Seeded file structure '{StructureKey}' with ID {Id}.",
            config.Key,
            entity.Id);
    }

    private async Task EnsureStructureRootFolderAsync(FileStructure structure)
    {
        var path = $"/{structure.Key}";
        var existingFolder = await _fileFolderRepository.FindByPathAsync(path, _currentTenant.Id);
        if (existingFolder != null)
        {
            existingFolder.Type = FolderType.Structure;
            existingFolder.StructureKey = structure.Key;
            existingFolder.Name = structure.DisplayName;
            existingFolder.ParentId = null;
            existingFolder.SetDisplayProperties("folder", null, structure.Description);
            await _fileFolderRepository.UpdateAsync(existingFolder, autoSave: true);
            return;
        }

        var folder = new FileFolder(
            _guidGenerator.Create(),
            _currentTenant.Id,
            structure.DisplayName,
            path,
            FolderType.Structure,
            structureKey: structure.Key);

        folder.SetDisplayProperties("folder", null, structure.Description);
        await _fileFolderRepository.InsertAsync(folder, autoSave: true);
    }
}

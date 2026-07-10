using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.FileManager;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.FileFolders;
using SufiChain.SufiAbp.FileManager.FileStructures;
using SufiChain.SufiAbp.LocalizationManagement;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.FileManager.Data;

/// <summary>
/// Seeds default file structures defined in FileManagerOptions.
/// This contributor is automatically discovered and executed by ABP's data seeding system.
/// </summary>
public class FileStructureDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IFileStructureRepository _fileStructureRepository;
    private readonly IFileFolderRepository _fileFolderRepository;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;
    private readonly FileManagerOptions _options;
    private readonly ILocalizationTextSeeder _localizationTextSeeder;
    private readonly ILogger<FileStructureDataSeedContributor> _logger;

    public FileStructureDataSeedContributor(
        IFileStructureRepository fileStructureRepository,
        IFileFolderRepository fileFolderRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IOptions<FileManagerOptions> options,
        ILocalizationTextSeeder localizationTextSeeder,
        ILogger<FileStructureDataSeedContributor> logger)
    {
        _fileStructureRepository = fileStructureRepository;
        _fileFolderRepository = fileFolderRepository;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
        _options = options.Value;
        _localizationTextSeeder = localizationTextSeeder;
        _logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        {
            _logger.LogInformation(
                "FileStructure seeding started. TenantId={TenantId}, SeedDefaultStructures={SeedDefaultStructures}",
                context?.TenantId, _options.SeedDefaultStructures);

            if (_options.SeedDefaultStructures)
            {
                _options.AddDefaultStructures();
            }

            if (_options.Structures.Count == 0)
            {
                _logger.LogInformation("No file structures configured to seed.");
                return;
            }

            await SeedGeneralStructureLocalizationAsync(context);

            _logger.LogInformation("Seeding {Count} file structure(s)...", _options.Structures.Count);

            foreach (var config in _options.Structures)
            {
                await SeedStructureAsync(config);
            }

            _logger.LogInformation("FileStructure seeding completed.");
        }
    }

    private async Task SeedGeneralStructureLocalizationAsync(DataSeedContext context)
    {
        if (!_options.Structures.Any(s => s.Key == FileStructureKeys.General))
        {
            return;
        }

        await _localizationTextSeeder.UpsertStructureTextsAsync(
            FileManagerFileStructureSeedTexts.ResourceName,
            FileManagerFileStructureSeedTexts.GeneralKey,
            FileManagerFileStructureSeedTexts.GeneralDisplayName,
            FileManagerFileStructureSeedTexts.GeneralDescription,
            context?.TenantId);
    }

    private async Task SeedStructureAsync(FileStructureConfig config)
    {
        var existing = await _fileStructureRepository.FindByKeyAsync(config.Key);

        if (existing != null)
        {
            await EnsureStaticStructurePropertiesAsync(existing, config);
            await EnsureStructureRootFolderAsync(existing);
            _logger.LogDebug("File structure '{StructureKey}' already exists with ID {Id}.", config.Key, existing.Id);
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
        _logger.LogInformation("Seeded file structure '{StructureKey}' with ID {Id}.", config.Key, entity.Id);
    }

    private async Task EnsureStructureRootFolderAsync(FileStructure structure)
    {
        var path = GetStructureRootPath(structure.Key);
        var existingFolder = await _fileFolderRepository.FindByPathAsync(path, _currentTenant.Id);
        if (existingFolder != null)
        {
            existingFolder.Type = FolderType.Structure;
            existingFolder.StructureKey = structure.Key;
            existingFolder.Name = GetStructureRootName(structure);
            existingFolder.ParentId = null;
            existingFolder.SetDisplayProperties("folder", null, structure.Description);
            await _fileFolderRepository.UpdateAsync(existingFolder, autoSave: true);
            return;
        }

        var folder = new FileFolder(
            _guidGenerator.Create(),
            _currentTenant.Id,
            GetStructureRootName(structure),
            path,
            FolderType.Structure,
            structureKey: structure.Key);

        folder.SetDisplayProperties("folder", null, structure.Description);
        await _fileFolderRepository.InsertAsync(folder, autoSave: true);
    }

    private static string GetStructureRootName(FileStructure structure) => structure.DisplayName;

    private static string GetStructureRootPath(string structureKey) => $"/{structureKey}";

    private async Task EnsureStaticStructurePropertiesAsync(FileStructure structure, FileStructureConfig config)
    {
        if (!config.IsStatic || structure.IsPublicAccess == config.IsPublicAccess)
        {
            return;
        }

        structure.IsPublicAccess = config.IsPublicAccess;
        await _fileStructureRepository.UpdateAsync(structure, autoSave: true);
    }
}

using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.Chat.Configuration;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.FileStructures;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Data;

/// <summary>
/// Seeds the Chat attachments file structure when FileManager is configured.
/// </summary>
public class ChatFileStructureDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected IFileStructureRepository FileStructureRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected ChatOptions ChatOptions { get; }
    protected FileManagerOptions FileManagerOptions { get; }
    protected ILogger<ChatFileStructureDataSeedContributor> Logger { get; }

    public ChatFileStructureDataSeedContributor(
        IFileStructureRepository fileStructureRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IOptions<ChatOptions> chatOptions,
        IOptions<FileManagerOptions> fileManagerOptions,
        ILogger<ChatFileStructureDataSeedContributor> logger)
    {
        FileStructureRepository = fileStructureRepository;
        GuidGenerator = guidGenerator;
        CurrentTenant = currentTenant;
        ChatOptions = chatOptions.Value;
        FileManagerOptions = fileManagerOptions.Value;
        Logger = logger;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        using (CurrentTenant.Change(context?.TenantId))
        {
            if (!ChatOptions.SeedFileStructure)
            {
                Logger.LogDebug("Chat file structure seeding is disabled. TenantId={TenantId}", context?.TenantId);
                return;
            }

            ChatOptions.AddDefaultFileStructure(FileManagerOptions);

            var config = FileManagerOptions.Structures
                .Find(structure => structure.Key == ChatFileStructureKeys.Attachments);

            if (config == null)
            {
                Logger.LogWarning("Chat attachments file structure configuration was not found.");
                return;
            }

            var existing = await FileStructureRepository.FindByKeyAsync(config.Key);
            if (existing != null)
            {
                return;
            }

            var entity = new FileStructure(
                GuidGenerator.Create(),
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

            await FileStructureRepository.InsertAsync(entity, autoSave: true);

            Logger.LogInformation(
                "Seeded Chat file structure '{StructureKey}' with ID {Id}.",
                config.Key,
                entity.Id);
        }
    }
}

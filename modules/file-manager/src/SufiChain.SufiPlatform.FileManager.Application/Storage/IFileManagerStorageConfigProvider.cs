using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Provides default storage configuration for internal use (e.g. blob container resolution during token-based downloads).
/// This provider has no authorization - it is used when resolving blob configuration for anonymous/token requests.
/// For admin UI, use IFileManagerStorageSettingsAppService which requires authorization.
/// </summary>
public interface IFileManagerStorageConfigProvider
{
    /// <summary>
    /// Gets the default storage configuration from settings. Used by StructureBlobContainerConfigurationProvider
    /// when the structure cache/repository does not have provider-specific config (e.g. fallback to defaults).
    /// </summary>
    Task<FileStructureStorageConfigDto> GetDefaultConfigAsync();
}

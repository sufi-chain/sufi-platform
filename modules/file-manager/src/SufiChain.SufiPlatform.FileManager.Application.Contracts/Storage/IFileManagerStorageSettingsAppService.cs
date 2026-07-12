using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Application service for default file-manager storage settings.
/// Used by Settings page and StructureBlobContainerConfigurationProvider.
/// </summary>
public interface IFileManagerStorageSettingsAppService : IApplicationService
{
    /// <summary>
    /// Gets the default storage configuration for the current tenant/global context.
    /// </summary>
    Task<FileStructureStorageConfigDto> GetDefaultConfigAsync();

    /// <summary>
    /// Updates the default storage configuration.
    /// </summary>
    Task UpdateDefaultConfigAsync(FileStructureStorageConfigDto input);

    /// <summary>
    /// Tests the connection to the specified storage provider using the given configuration.
    /// </summary>
    Task<TestStorageConnectionResult> TestConnectionAsync(TestStorageConnectionInput input);
}

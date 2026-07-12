using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.Settings;

/// <summary>
/// Application service for managing File Manager general and archiving settings.
/// </summary>
public interface IFileManagerSettingsAppService : IApplicationService
{
    Task<FileManagerGeneralSettingsDto> GetGeneralSettingsAsync();

    Task UpdateGeneralSettingsAsync(FileManagerGeneralSettingsDto input);

    Task<FileManagerArchivingSettingsDto> GetArchivingSettingsAsync();

    Task UpdateArchivingSettingsAsync(FileManagerArchivingSettingsDto input);
}

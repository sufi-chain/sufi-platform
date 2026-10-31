using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.Settings;

public interface IFileArchivingSettingsAppService : IApplicationService
{
    Task<FileManagerArchivingSettingsDto> GetAsync();
    Task UpdateAsync(FileManagerArchivingSettingsDto input);
}

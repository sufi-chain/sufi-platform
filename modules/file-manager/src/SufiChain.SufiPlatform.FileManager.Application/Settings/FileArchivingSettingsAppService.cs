using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.FileManager.Settings;

/// <summary>Saves file-archiving settings. Storage settings stay on the file manager section.</summary>
[RequiresFeature(SufiFileManagerFeatures.Enable)]
[Authorize(FileManagerPermissions.ArchivingSettings.Manage)]
public class FileArchivingSettingsAppService : SufiApplicationService, IFileArchivingSettingsAppService
{
    private readonly ISettingProvider _settingProvider;
    private readonly ISettingManager _settingManager;

    public FileArchivingSettingsAppService(ISettingProvider settingProvider, ISettingManager settingManager)
    {
        _settingProvider = settingProvider;
        _settingManager = settingManager;
    }

    public virtual async Task<FileManagerArchivingSettingsDto> GetAsync()
    {
        var aiRetentionDays = await _settingProvider.GetOrNullAsync(FileArchivingSettings.AIFilesRetentionDays);
        return new FileManagerArchivingSettingsDto
        {
            Enabled = await _settingProvider.GetAsync<bool>(FileArchivingSettings.Enabled),
            RetentionDays = await _settingProvider.GetAsync<int>(FileArchivingSettings.RetentionDays),
            BatchSize = await _settingProvider.GetAsync<int>(FileArchivingSettings.BatchSize),
            Schedule = await _settingProvider.GetOrNullAsync(FileArchivingSettings.Schedule) ?? "0 2 * * *",
            ArchiveAIFiles = await _settingProvider.GetAsync<bool>(FileArchivingSettings.ArchiveAIFiles),
            AIFilesRetentionDays = string.IsNullOrWhiteSpace(aiRetentionDays) ? null : int.Parse(aiRetentionDays)
        };
    }

    public virtual async Task UpdateAsync(FileManagerArchivingSettingsDto input)
    {
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.Enabled, input.Enabled.ToString());
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.RetentionDays, input.RetentionDays.ToString());
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.BatchSize, input.BatchSize.ToString());
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.Schedule, input.Schedule);
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.ArchiveAIFiles, input.ArchiveAIFiles.ToString());
        await _settingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.AIFilesRetentionDays, input.AIFilesRetentionDays?.ToString());
    }
}

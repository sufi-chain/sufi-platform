using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Permissions;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Application.Services;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.FileManager.Settings;

/// <summary>
/// Manages File Manager general and archiving settings.
/// </summary>
[RequiresFeature(SufiFileManagerFeatures.Enable)]
[Authorize(FileManagerPermissions.Settings.Default)]
public class FileManagerSettingsAppService : SufiApplicationService, IFileManagerSettingsAppService
{
    protected ISettingProvider SettingProvider { get; }
    protected ISettingManager SettingManager { get; }

    public FileManagerSettingsAppService(
        ISettingProvider settingProvider,
        ISettingManager settingManager)
    {
        SettingProvider = settingProvider;
        SettingManager = settingManager;
    }

    public virtual async Task<FileManagerGeneralSettingsDto> GetGeneralSettingsAsync()
    {
        return new FileManagerGeneralSettingsDto
        {
            StorageQuotaMB = await SettingProvider.GetAsync<long>(FileManagerSettings.StorageQuota),
            MaxFileSizeBytes = await SettingProvider.GetAsync<long>(FileManagerSettings.MaxFileSize),
            AllowedImageExtensions = await SettingProvider.GetOrNullAsync(FileManagerSettings.AllowedImageExtensions) ?? string.Empty,
            AllowedVideoExtensions = await SettingProvider.GetOrNullAsync(FileManagerSettings.AllowedVideoExtensions) ?? string.Empty,
            AllowedDocumentExtensions = await SettingProvider.GetOrNullAsync(FileManagerSettings.AllowedDocumentExtensions) ?? string.Empty,
            EnableWebPConversion = await SettingProvider.GetAsync<bool>(FileManagerSettings.EnableWebPConversion),
            WebPQuality = await SettingProvider.GetAsync<int>(FileManagerSettings.WebPQuality),
            ThumbnailWidth = await SettingProvider.GetAsync<int>(FileManagerSettings.ThumbnailWidth),
            ThumbnailHeight = await SettingProvider.GetAsync<int>(FileManagerSettings.ThumbnailHeight),
            MaxImageWidth = await SettingProvider.GetAsync<int>(FileManagerSettings.MaxImageWidth),
            MaxImageHeight = await SettingProvider.GetAsync<int>(FileManagerSettings.MaxImageHeight),
            AutoDeleteTempMediaAfterDays = await SettingProvider.GetAsync<int>(FileManagerSettings.AutoDeleteTempMediaAfterDays),
            EnableDuplicateDetection = await SettingProvider.GetAsync<bool>(FileManagerSettings.EnableDuplicateDetection)
        };
    }

    public virtual async Task UpdateGeneralSettingsAsync(FileManagerGeneralSettingsDto input)
    {
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.StorageQuota, input.StorageQuotaMB.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.MaxFileSize, input.MaxFileSizeBytes.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.AllowedImageExtensions, input.AllowedImageExtensions);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.AllowedVideoExtensions, input.AllowedVideoExtensions);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.AllowedDocumentExtensions, input.AllowedDocumentExtensions);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.EnableWebPConversion, input.EnableWebPConversion.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.WebPQuality, input.WebPQuality.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.ThumbnailWidth, input.ThumbnailWidth.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.ThumbnailHeight, input.ThumbnailHeight.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.MaxImageWidth, input.MaxImageWidth.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.MaxImageHeight, input.MaxImageHeight.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.AutoDeleteTempMediaAfterDays, input.AutoDeleteTempMediaAfterDays.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileManagerSettings.EnableDuplicateDetection, input.EnableDuplicateDetection.ToString());
    }

    public virtual async Task<FileManagerArchivingSettingsDto> GetArchivingSettingsAsync()
    {
        var aiRetentionDaysStr = await SettingProvider.GetOrNullAsync(FileArchivingSettings.AIFilesRetentionDays);

        return new FileManagerArchivingSettingsDto
        {
            Enabled = await SettingProvider.GetAsync<bool>(FileArchivingSettings.Enabled),
            RetentionDays = await SettingProvider.GetAsync<int>(FileArchivingSettings.RetentionDays),
            BatchSize = await SettingProvider.GetAsync<int>(FileArchivingSettings.BatchSize),
            Schedule = await SettingProvider.GetOrNullAsync(FileArchivingSettings.Schedule) ?? "0 2 * * *",
            ArchiveAIFiles = await SettingProvider.GetAsync<bool>(FileArchivingSettings.ArchiveAIFiles),
            AIFilesRetentionDays = string.IsNullOrWhiteSpace(aiRetentionDaysStr) ? null : int.Parse(aiRetentionDaysStr)
        };
    }

    public virtual async Task UpdateArchivingSettingsAsync(FileManagerArchivingSettingsDto input)
    {
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.Enabled, input.Enabled.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.RetentionDays, input.RetentionDays.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.BatchSize, input.BatchSize.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.Schedule, input.Schedule);
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.ArchiveAIFiles, input.ArchiveAIFiles.ToString());
        await SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, FileArchivingSettings.AIFilesRetentionDays,
            input.AIFilesRetentionDays?.ToString());
    }
}
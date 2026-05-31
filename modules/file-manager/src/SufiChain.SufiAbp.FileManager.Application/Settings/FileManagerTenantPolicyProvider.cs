using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.FileManager.Configuration;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.FileManager.Settings;

/// <summary>
/// Resolves tenant File Manager policy from settings with host option fallbacks.
/// </summary>
public interface IFileManagerTenantPolicyProvider
{
    Task<FileManagerGeneralSettingsDto> GetGeneralPolicyAsync();
}

/// <inheritdoc />
public class FileManagerTenantPolicyProvider : IFileManagerTenantPolicyProvider, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }
    protected FileManagerOptions Options { get; }

    public FileManagerTenantPolicyProvider(
        ISettingProvider settingProvider,
        IOptions<FileManagerOptions> options)
    {
        SettingProvider = settingProvider;
        Options = options.Value;
    }

    public virtual async Task<FileManagerGeneralSettingsDto> GetGeneralPolicyAsync()
    {
        var storageQuota = await SettingProvider.GetAsync<long>(FileManagerSettings.StorageQuota);
        if (storageQuota == 0 && Options.DefaultStorageQuotaMB > 0)
        {
            storageQuota = Options.DefaultStorageQuotaMB;
        }

        var maxFileSize = await SettingProvider.GetAsync<long>(FileManagerSettings.MaxFileSize);
        if (maxFileSize <= 0)
        {
            maxFileSize = Options.MaxUploadFileSizeMB * 1024L * 1024L;
        }

        var autoDeleteDays = await SettingProvider.GetAsync<int>(FileManagerSettings.AutoDeleteTempMediaAfterDays);
        if (autoDeleteDays <= 0)
        {
            autoDeleteDays = 7;
        }

        return new FileManagerGeneralSettingsDto
        {
            StorageQuotaMB = storageQuota,
            MaxFileSizeBytes = maxFileSize,
            AllowedImageExtensions = await SettingProvider.GetOrNullAsync(FileManagerSettings.AllowedImageExtensions)
                ?? "jpg,jpeg,png,gif,webp,svg",
            AllowedVideoExtensions = await SettingProvider.GetOrNullAsync(FileManagerSettings.AllowedVideoExtensions)
                ?? "mp4,webm,ogg,mov,avi",
            AllowedDocumentExtensions = await SettingProvider.GetOrNullAsync(FileManagerSettings.AllowedDocumentExtensions)
                ?? "pdf,doc,docx,xls,xlsx,ppt,pptx,txt",
            EnableWebPConversion = await SettingProvider.GetAsync<bool>(FileManagerSettings.EnableWebPConversion),
            WebPQuality = await SettingProvider.GetAsync<int>(FileManagerSettings.WebPQuality),
            ThumbnailWidth = await SettingProvider.GetAsync<int>(FileManagerSettings.ThumbnailWidth),
            ThumbnailHeight = await SettingProvider.GetAsync<int>(FileManagerSettings.ThumbnailHeight),
            MaxImageWidth = await SettingProvider.GetAsync<int>(FileManagerSettings.MaxImageWidth),
            MaxImageHeight = await SettingProvider.GetAsync<int>(FileManagerSettings.MaxImageHeight),
            AutoDeleteTempMediaAfterDays = autoDeleteDays,
            EnableDuplicateDetection = await SettingProvider.GetAsync<bool>(FileManagerSettings.EnableDuplicateDetection)
        };
    }
}

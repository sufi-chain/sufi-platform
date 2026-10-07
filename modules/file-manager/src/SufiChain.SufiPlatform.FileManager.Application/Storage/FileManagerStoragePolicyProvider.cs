using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class FileManagerStoragePolicyProvider :
    IFileManagerStoragePolicyProvider,
    ITransientDependency
{
    private readonly IFeatureChecker _featureChecker;
    private readonly ISettingProvider _settingProvider;

    public FileManagerStoragePolicyProvider(IFeatureChecker featureChecker, ISettingProvider settingProvider)
    {
        _featureChecker = featureChecker;
        _settingProvider = settingProvider;
    }

    public virtual async Task<FileManagerStoragePolicy> GetAsync(
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var providerValue = await _featureChecker.GetOrNullAsync(
            SufiFileManagerFeatures.Storage.Provider);
        if (!Enum.TryParse(
                providerValue,
                ignoreCase: true,
                out FileStructureStorageProvider provider))
        {
            provider = FileStructureStorageProvider.Database;
        }

        var maximumBytesValue = await _featureChecker.GetOrNullAsync(
            SufiFileManagerFeatures.Storage.MaxBytes);
        if (!long.TryParse(
                maximumBytesValue,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out var entitlementBytes) ||
            entitlementBytes < 0)
        {
            entitlementBytes = long.Parse(
                SufiFileManagerFeatures.Storage.DefaultMaxBytes,
                CultureInfo.InvariantCulture);
        }

        var quotaSetting = await _settingProvider.GetOrNullAsync(FileManagerSettings.StorageQuota);
        long settingBytes = 0;
        if (long.TryParse(quotaSetting, NumberStyles.None, CultureInfo.InvariantCulture, out var quotaMegabytes) &&
            quotaMegabytes > 0)
        {
            settingBytes = quotaMegabytes * 1024L * 1024L;
        }

        var maximumBytes = entitlementBytes > 0 && settingBytes > 0
            ? Math.Min(entitlementBytes, settingBytes)
            : entitlementBytes > 0 ? entitlementBytes : settingBytes;

        return new FileManagerStoragePolicy
        {
            Provider = provider,
            MaxStorageBytes = maximumBytes
        };
    }
}

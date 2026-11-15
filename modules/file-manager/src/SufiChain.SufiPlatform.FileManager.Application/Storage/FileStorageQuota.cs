using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public interface IFileStorageQuota
{
    /// <summary>
    /// Effective byte limit for the current host or tenant. Read from ABP features on every call.
    /// </summary>
    Task<long> GetLimitBytesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Limit and usage for the current host or tenant. The upload dialog and the quota guard both use this.
    /// </summary>
    Task<FileStorageQuotaSnapshot> GetAsync(CancellationToken cancellationToken = default);
}

public sealed class FileStorageQuotaSnapshot
{
    public Guid? TenantId { get; init; }

    public long UsedBytes { get; init; }

    public long LimitBytes { get; init; }

    public bool IsUnlimited => LimitBytes <= 0;

    public StorageQuotaDto ToDto()
    {
        var availableBytes = LimitBytes > 0
            ? Math.Max(0, LimitBytes - UsedBytes)
            : long.MaxValue;

        return new StorageQuotaDto
        {
            IsUnlimited = IsUnlimited,
            UsedBytes = UsedBytes,
            LimitBytes = LimitBytes,
            AvailableBytes = availableBytes,
            UsedMB = UsedBytes / (1024.0 * 1024.0),
            LimitMB = LimitBytes / (1024.0 * 1024.0),
            AvailableMB = LimitBytes > 0
                ? availableBytes / (1024.0 * 1024.0)
                : 0,
            PercentageUsed = LimitBytes > 0
                ? Math.Min(100, UsedBytes / (double)LimitBytes * 100)
                : 0
        };
    }
}

/// <summary>
/// One quota reading for the upload dialog and <see cref="FileStorageQuotaGuard"/>.
/// The limit is the ABP feature <c>SufiFileManager.Storage.MaxBytes</c> (host value when no
/// tenant is current, otherwise the tenant value), or the feature default when that value
/// is missing. An optional <see cref="FileManagerSettings.StorageQuota"/> cap in megabytes
/// still narrows the feature entitlement. Nothing here is cached; feature edits show up
/// after ABP's feature value cache invalidation.
/// </summary>
public class FileStorageQuota : IFileStorageQuota, ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IFeatureChecker _featureChecker;
    private readonly ISettingProvider _settingProvider;

    public FileStorageQuota(
        ICurrentTenant currentTenant,
        IFileItemRepository fileItemRepository,
        IFeatureChecker featureChecker,
        ISettingProvider settingProvider)
    {
        _currentTenant = currentTenant;
        _fileItemRepository = fileItemRepository;
        _featureChecker = featureChecker;
        _settingProvider = settingProvider;
    }

    public virtual async Task<long> GetLimitBytesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

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

        if (entitlementBytes > 0 && settingBytes > 0)
        {
            return Math.Min(entitlementBytes, settingBytes);
        }

        return entitlementBytes > 0 ? entitlementBytes : settingBytes;
    }

    public virtual async Task<FileStorageQuotaSnapshot> GetAsync(CancellationToken cancellationToken = default)
    {
        var limitBytes = await GetLimitBytesAsync(cancellationToken);
        var usedBytes = await _fileItemRepository.GetTotalSizeByTenantAsync(
            _currentTenant.Id,
            cancellationToken);

        return new FileStorageQuotaSnapshot
        {
            TenantId = _currentTenant.Id,
            UsedBytes = usedBytes,
            LimitBytes = limitBytes
        };
    }
}

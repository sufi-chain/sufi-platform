using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class FileStorageQuotaGuard : IFileStorageQuotaGuard, ITransientDependency
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    private readonly ICurrentTenant _currentTenant;
    private readonly IFileStorageQuota _fileStorageQuota;
    private readonly IAbpDistributedLock _distributedLock;

    public FileStorageQuotaGuard(
        ICurrentTenant currentTenant,
        IFileStorageQuota fileStorageQuota,
        IAbpDistributedLock distributedLock)
    {
        _currentTenant = currentTenant;
        _fileStorageQuota = fileStorageQuota;
        _distributedLock = distributedLock;
    }

    public Task ExecuteAsync(
        long positiveByteDelta,
        Func<Task> action,
        CancellationToken cancellationToken = default)
    {
        return ExecuteAsync(
            positiveByteDelta,
            async () =>
            {
                await action();
                return true;
            },
            cancellationToken);
    }

    public virtual async Task<TResult> ExecuteAsync<TResult>(
        long positiveByteDelta,
        Func<Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        if (positiveByteDelta <= 0)
        {
            return await action();
        }

        var scope = _currentTenant.Id?.ToString() ?? "host";
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            $"SufiFileManager:StorageQuota:{scope}",
            LockTimeout,
            cancellationToken);

        if (lockHandle == null)
        {
            var timeout = new BusinessException(FileManagerErrorCodes.StorageQuotaLockTimeout);
            if (_currentTenant.Id.HasValue)
            {
                timeout.WithData("TenantId", _currentTenant.Id.Value);
            }

            throw timeout;
        }

        var quota = await _fileStorageQuota.GetAsync(cancellationToken);
        if (quota.LimitBytes > 0 &&
            positiveByteDelta > quota.LimitBytes - quota.UsedBytes)
        {
            throw CreateQuotaException(
                _currentTenant.Id,
                positiveByteDelta,
                quota.UsedBytes,
                quota.LimitBytes);
        }

        return await action();
    }

    private static BusinessException CreateQuotaException(
        Guid? tenantId,
        long requestedBytes,
        long usedBytes,
        long maximumBytes)
    {
        var exception = new BusinessException(FileManagerErrorCodes.StorageQuotaExceeded);
        if (tenantId.HasValue)
        {
            exception.WithData("TenantId", tenantId.Value);
        }

        exception.WithData("RequestedBytes", requestedBytes);
        exception.WithData("UsedBytes", usedBytes);
        exception.WithData("MaximumBytes", maximumBytes);
        return exception;
    }
}

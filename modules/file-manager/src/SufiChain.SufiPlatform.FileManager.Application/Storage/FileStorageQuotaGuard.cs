using System;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileItems;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class FileStorageQuotaGuard : IFileStorageQuotaGuard, ITransientDependency
{
    private static readonly TimeSpan LockTimeout = TimeSpan.FromSeconds(30);

    private readonly ICurrentTenant _currentTenant;
    private readonly IFileItemRepository _fileItemRepository;
    private readonly IFileManagerStoragePolicyProvider _storagePolicyProvider;
    private readonly IAbpDistributedLock _distributedLock;

    public FileStorageQuotaGuard(
        ICurrentTenant currentTenant,
        IFileItemRepository fileItemRepository,
        IFileManagerStoragePolicyProvider storagePolicyProvider,
        IAbpDistributedLock distributedLock)
    {
        _currentTenant = currentTenant;
        _fileItemRepository = fileItemRepository;
        _storagePolicyProvider = storagePolicyProvider;
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
        if (positiveByteDelta <= 0 || !_currentTenant.Id.HasValue)
        {
            return await action();
        }

        var tenantId = _currentTenant.Id.Value;
        await using var lockHandle = await _distributedLock.TryAcquireAsync(
            $"SufiFileManager:StorageQuota:{tenantId}",
            LockTimeout,
            cancellationToken);

        if (lockHandle == null)
        {
            throw new BusinessException(FileManagerErrorCodes.StorageQuotaLockTimeout)
                .WithData("TenantId", tenantId);
        }

        var policy = await _storagePolicyProvider.GetAsync(cancellationToken);
        if (policy.MaxStorageBytes > 0)
        {
            var usedBytes = await _fileItemRepository.GetTotalSizeByTenantAsync(tenantId, cancellationToken);
            if (positiveByteDelta > policy.MaxStorageBytes - usedBytes)
            {
                throw CreateQuotaException(
                    tenantId,
                    positiveByteDelta,
                    usedBytes,
                    policy.MaxStorageBytes);
            }
        }

        return await action();
    }

    private static BusinessException CreateQuotaException(
        Guid tenantId,
        long requestedBytes,
        long usedBytes,
        long maximumBytes)
    {
        var exception = new BusinessException(FileManagerErrorCodes.StorageQuotaExceeded);
        exception.WithData("TenantId", tenantId);
        exception.WithData("RequestedBytes", requestedBytes);
        exception.WithData("UsedBytes", usedBytes);
        exception.WithData("MaximumBytes", maximumBytes);
        return exception;
    }
}

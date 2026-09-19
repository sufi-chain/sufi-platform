using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

public class FileStorageQuotaGuardTests
{
    [Fact]
    public async Task Host_Write_Should_Skip_Quota()
    {
        var ran = false;
        var repository = Substitute.For<IFileItemRepository>();
        var sut = CreateSut(tenantId: null, repository);

        await sut.ExecuteAsync(5_000, () =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        ran.ShouldBeTrue();
        await repository.DidNotReceive().GetTotalSizeByTenantAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Zero_Or_Negative_Delta_Should_Skip_Quota()
    {
        var tenantId = Guid.NewGuid();
        var repository = Substitute.For<IFileItemRepository>();
        var sut = CreateSut(tenantId, repository);

        await sut.ExecuteAsync(0, () => Task.CompletedTask);
        await sut.ExecuteAsync(-10, () => Task.CompletedTask);

        await repository.DidNotReceive().GetTotalSizeByTenantAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Exceeding_Quota_Should_Throw()
    {
        var tenantId = Guid.NewGuid();
        var repository = Substitute.For<IFileItemRepository>();
        repository.GetTotalSizeByTenantAsync(tenantId, Arg.Any<CancellationToken>()).Returns(90);
        var sut = CreateSut(tenantId, repository, maxStorageBytes: 100);

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            sut.ExecuteAsync(20, () => Task.CompletedTask));

        ex.Code.ShouldBe(FileManagerErrorCodes.StorageQuotaExceeded);
    }

    [Fact]
    public async Task Within_Quota_Should_Run_Action()
    {
        var tenantId = Guid.NewGuid();
        var repository = Substitute.For<IFileItemRepository>();
        repository.GetTotalSizeByTenantAsync(tenantId, Arg.Any<CancellationToken>()).Returns(10);
        var sut = CreateSut(tenantId, repository, maxStorageBytes: 100);
        var ran = false;

        await sut.ExecuteAsync(20, () =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        ran.ShouldBeTrue();
    }

    [Fact]
    public async Task Lock_Timeout_Should_Throw()
    {
        var tenantId = Guid.NewGuid();
        var repository = Substitute.For<IFileItemRepository>();
        var sut = CreateSut(tenantId, repository, acquireLock: false);

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            sut.ExecuteAsync(1, () => Task.CompletedTask));

        ex.Code.ShouldBe(FileManagerErrorCodes.StorageQuotaLockTimeout);
    }

    private static FileStorageQuotaGuard CreateSut(
        Guid? tenantId,
        IFileItemRepository repository,
        long maxStorageBytes = 100,
        bool acquireLock = true)
    {
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);

        var policy = Substitute.For<IFileManagerStoragePolicyProvider>();
        policy.GetAsync(Arg.Any<CancellationToken>())
            .Returns(new FileManagerStoragePolicy
            {
                Provider = FileStructureStorageProvider.Database,
                MaxStorageBytes = maxStorageBytes
            });

        var distributedLock = Substitute.For<IAbpDistributedLock>();
        IAbpDistributedLockHandle? handle = acquireLock ? Substitute.For<IAbpDistributedLockHandle>() : null;
        distributedLock.TryAcquireAsync(
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(handle);

        return new FileStorageQuotaGuard(currentTenant, repository, policy, distributedLock);
    }
}

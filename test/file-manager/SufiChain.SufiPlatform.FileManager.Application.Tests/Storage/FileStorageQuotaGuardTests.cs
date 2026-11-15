using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.FileManager.Features;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Settings;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp;
using Volo.Abp.DistributedLocking;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.FileManager;

public class FileStorageQuotaGuardTests
{
    [Fact]
    public async Task Host_upload_over_the_limit_is_rejected()
    {
        var quota = CreateQuota(tenantId: null, usedBytes: 90, featureBytes: "100");
        var sut = CreateGuard(quota);

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            sut.ExecuteAsync(20, () => Task.CompletedTask));

        ex.Code.ShouldBe(FileManagerErrorCodes.StorageQuotaExceeded);
        ex.Data["MaximumBytes"].ShouldBe(100L);
        ex.Data["UsedBytes"].ShouldBe(90L);
    }

    [Fact]
    public async Task Host_upload_under_the_limit_is_accepted()
    {
        var quota = CreateQuota(tenantId: null, usedBytes: 10, featureBytes: "100");
        var sut = CreateGuard(quota);
        var ran = false;

        await sut.ExecuteAsync(20, () =>
        {
            ran = true;
            return Task.CompletedTask;
        });

        ran.ShouldBeTrue();
    }

    [Fact]
    public async Task Tenant_upload_over_the_limit_is_still_rejected()
    {
        var tenantId = Guid.NewGuid();
        var quota = CreateQuota(tenantId, usedBytes: 90, featureBytes: "100");
        var sut = CreateGuard(quota);

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            sut.ExecuteAsync(20, () => Task.CompletedTask));

        ex.Code.ShouldBe(FileManagerErrorCodes.StorageQuotaExceeded);
        ex.Data["TenantId"].ShouldBe(tenantId);
    }

    [Fact]
    public async Task Dialog_and_guard_report_the_same_limit()
    {
        var quota = CreateQuota(tenantId: null, usedBytes: 103 * 1024 * 1024, featureBytes: "100000000");
        var dialog = (await quota.Quota.GetAsync()).ToDto();
        var sut = CreateGuard(quota);

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            sut.ExecuteAsync(11, () => Task.CompletedTask));

        dialog.LimitBytes.ShouldBe(100_000_000L);
        ex.Data["MaximumBytes"].ShouldBe(dialog.LimitBytes);
        ex.Data["UsedBytes"].ShouldBe(dialog.UsedBytes);
        dialog.UsedBytes.ShouldBe(103L * 1024 * 1024);
    }

    [Fact]
    public async Task Feature_value_change_is_visible_without_a_private_cache()
    {
        var reads = 0;
        var quota = CreateQuota(
            tenantId: null,
            usedBytes: 0,
            featureBytes: "100000000",
            onFeatureRead: () =>
            {
                reads++;
                return reads == 1 ? "100000000" : "1073741824";
            });

        var before = await quota.Quota.GetLimitBytesAsync();
        var after = await quota.Quota.GetLimitBytesAsync();

        before.ShouldBe(100_000_000L);
        after.ShouldBe(1_073_741_824L);
        reads.ShouldBe(2);
    }

    [Fact]
    public async Task Zero_Or_Negative_Delta_Should_Skip_Quota()
    {
        var tenantId = Guid.NewGuid();
        var quota = CreateQuota(tenantId, usedBytes: 90, featureBytes: "100");
        var sut = CreateGuard(quota);

        await sut.ExecuteAsync(0, () => Task.CompletedTask);
        await sut.ExecuteAsync(-10, () => Task.CompletedTask);

        await quota.Repository.DidNotReceive().GetTotalSizeByTenantAsync(
            Arg.Any<Guid?>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Within_Quota_Should_Run_Action()
    {
        var tenantId = Guid.NewGuid();
        var quota = CreateQuota(tenantId, usedBytes: 10, featureBytes: "100");
        var sut = CreateGuard(quota);
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
        var quota = CreateQuota(tenantId, usedBytes: 0, featureBytes: "100");
        var sut = CreateGuard(quota, acquireLock: false);

        var ex = await Should.ThrowAsync<BusinessException>(() =>
            sut.ExecuteAsync(1, () => Task.CompletedTask));

        ex.Code.ShouldBe(FileManagerErrorCodes.StorageQuotaLockTimeout);
    }

    private static FileStorageQuotaGuard CreateGuard(QuotaFixture quota, bool acquireLock = true)
    {
        var distributedLock = Substitute.For<IAbpDistributedLock>();
        IAbpDistributedLockHandle? handle = acquireLock ? Substitute.For<IAbpDistributedLockHandle>() : null;
        distributedLock.TryAcquireAsync(
                Arg.Any<string>(),
                Arg.Any<TimeSpan>(),
                Arg.Any<CancellationToken>())
            .Returns(handle);

        return new FileStorageQuotaGuard(quota.CurrentTenant, quota.Quota, distributedLock);
    }

    private static QuotaFixture CreateQuota(
        Guid? tenantId,
        long usedBytes,
        string featureBytes,
        Func<string>? onFeatureRead = null)
    {
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);

        var repository = Substitute.For<IFileItemRepository>();
        repository.GetTotalSizeByTenantAsync(tenantId, Arg.Any<CancellationToken>()).Returns(usedBytes);

        var features = Substitute.For<IFeatureChecker>();
        features.GetOrNullAsync(SufiFileManagerFeatures.Storage.MaxBytes)
            .Returns(_ => onFeatureRead?.Invoke() ?? featureBytes);

        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(FileManagerSettings.StorageQuota).Returns((string?)null);

        return new QuotaFixture(
            currentTenant,
            repository,
            new FileStorageQuota(currentTenant, repository, features, settings));
    }

    private sealed class QuotaFixture
    {
        public QuotaFixture(
            ICurrentTenant currentTenant,
            IFileItemRepository repository,
            FileStorageQuota quota)
        {
            CurrentTenant = currentTenant;
            Repository = repository;
            Quota = quota;
        }

        public ICurrentTenant CurrentTenant { get; }

        public IFileItemRepository Repository { get; }

        public FileStorageQuota Quota { get; }
    }
}

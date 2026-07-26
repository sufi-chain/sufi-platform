using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.Identity.LinkUsers;

public class IdentityLinkUserManager_Tests
{
    private readonly IIdentityLinkUserRepository _repository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IdentityLinkUserManager _manager;
    private readonly List<IdentityLinkUser> _store = new();

    public IdentityLinkUserManager_Tests()
    {
        _repository = Substitute.For<IIdentityLinkUserRepository>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _currentTenant.Change(Arg.Any<Guid?>()).Returns(Substitute.For<IDisposable>());

        _repository.FindAsync(Arg.Any<IdentityLinkUserInfo>(), Arg.Any<IdentityLinkUserInfo>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var source = ci.ArgAt<IdentityLinkUserInfo>(0);
                var target = ci.ArgAt<IdentityLinkUserInfo>(1);
                return _store.Find(x =>
                    x.SourceUserId == source.UserId && x.SourceTenantId == source.TenantId &&
                    x.TargetUserId == target.UserId && x.TargetTenantId == target.TenantId ||
                    x.TargetUserId == source.UserId && x.TargetTenantId == source.TenantId &&
                    x.SourceUserId == target.UserId && x.SourceTenantId == target.TenantId);
            });

        _repository.GetListAsync(Arg.Any<IdentityLinkUserInfo>(), Arg.Any<List<IdentityLinkUserInfo>?>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var info = ci.ArgAt<IdentityLinkUserInfo>(0);
                return _store.FindAll(x =>
                    x.SourceUserId == info.UserId && x.SourceTenantId == info.TenantId ||
                    x.TargetUserId == info.UserId && x.TargetTenantId == info.TenantId);
            });

        _repository.InsertAsync(Arg.Any<IdentityLinkUser>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var link = ci.ArgAt<IdentityLinkUser>(0);
                _store.Add(link);
                return link;
            });

        _repository.DeleteAsync(Arg.Any<IdentityLinkUser>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(ci =>
            {
                var link = ci.ArgAt<IdentityLinkUser>(0);
                _store.Remove(link);
                return Task.CompletedTask;
            });

        var lazy = Substitute.For<IAbpLazyServiceProvider>();
        lazy.LazyGetService(Arg.Any<IGuidGenerator>())
            .Returns(callInfo => callInfo.ArgAt<IGuidGenerator>(0));

        _manager = new IdentityLinkUserManager(_repository, userManager: null!, _currentTenant)
        {
            LazyServiceProvider = lazy
        };
    }

    [Fact]
    public async Task LinkAsync_Should_Insert_Host_Scoped_Link()
    {
        var hostUser = new IdentityLinkUserInfo(Guid.NewGuid());
        var tenantUser = new IdentityLinkUserInfo(Guid.NewGuid(), Guid.NewGuid());

        await _manager.LinkAsync(hostUser, tenantUser);

        _store.Count.ShouldBe(1);
        _store[0].SourceUserId.ShouldBe(hostUser.UserId);
        _store[0].SourceTenantId.ShouldBeNull();
        _store[0].TargetUserId.ShouldBe(tenantUser.UserId);
        _store[0].TargetTenantId.ShouldBe(tenantUser.TenantId);
        _currentTenant.Received().Change(null);
    }

    [Fact]
    public async Task LinkAsync_Should_Be_Idempotent()
    {
        var a = new IdentityLinkUserInfo(Guid.NewGuid());
        var b = new IdentityLinkUserInfo(Guid.NewGuid(), Guid.NewGuid());

        await _manager.LinkAsync(a, b);
        await _manager.LinkAsync(a, b);

        _store.Count.ShouldBe(1);
    }

    [Fact]
    public async Task IsLinkedAsync_Should_Be_Bidirectional()
    {
        var a = new IdentityLinkUserInfo(Guid.NewGuid());
        var b = new IdentityLinkUserInfo(Guid.NewGuid(), Guid.NewGuid());

        await _manager.LinkAsync(a, b);

        (await _manager.IsLinkedAsync(a, b)).ShouldBeTrue();
        (await _manager.IsLinkedAsync(b, a)).ShouldBeTrue();
    }

    [Fact]
    public async Task UnlinkAsync_Should_Remove_Link()
    {
        var a = new IdentityLinkUserInfo(Guid.NewGuid());
        var b = new IdentityLinkUserInfo(Guid.NewGuid(), Guid.NewGuid());

        await _manager.LinkAsync(a, b);
        await _manager.UnlinkAsync(a, b);

        _store.Count.ShouldBe(0);
        (await _manager.IsLinkedAsync(a, b)).ShouldBeFalse();
    }

    [Fact]
    public async Task GetListAsync_IncludeIndirect_Should_Walk_Graph()
    {
        var a = new IdentityLinkUserInfo(Guid.NewGuid());
        var b = new IdentityLinkUserInfo(Guid.NewGuid());
        var c = new IdentityLinkUserInfo(Guid.NewGuid());

        await _manager.LinkAsync(a, b);
        await _manager.LinkAsync(b, c);

        _repository.GetListAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(_ => _store.ToList());

        var related = await _manager.GetListAsync(a, includeIndirect: true);
        related.Count.ShouldBe(2);
    }
}

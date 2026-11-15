using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using Volo.Abp.Caching;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomainStorePrimaryHostTests
{
    private static readonly Guid TenantId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    [Fact]
    public async Task Ignores_unverified_domains()
    {
        var tenant = CreateTenant();
        tenant.ConfigureRouting(
            "acme",
            [
                CreateDomain("unverified.platform.test", TenantDomainType.Generated, isVerified: false, isActive: true),
                CreateDomain("acme.platform.test", TenantDomainType.Generated, isVerified: true, isActive: true)
            ]);

        var host = await CreateStore(tenant).FindPrimaryHostAsync(TenantId);

        host.ShouldBe("acme.platform.test");
    }

    [Fact]
    public async Task Ignores_inactive_domains()
    {
        var tenant = CreateTenant();
        tenant.ConfigureRouting(
            "acme",
            [
                CreateDomain("inactive.example", TenantDomainType.Custom, isVerified: true, isActive: false),
                CreateDomain("acme.platform.test", TenantDomainType.Generated, isVerified: true, isActive: true)
            ]);

        var host = await CreateStore(tenant).FindPrimaryHostAsync(TenantId);

        host.ShouldBe("acme.platform.test");
    }

    [Fact]
    public async Task Custom_beats_generated()
    {
        var tenant = CreateTenant();
        tenant.ConfigureRouting(
            "acme",
            [
                CreateDomain("acme.platform.test", TenantDomainType.Generated, isVerified: true, isActive: true),
                CreateDomain("shop.example", TenantDomainType.Custom, isVerified: true, isActive: true)
            ]);

        var host = await CreateStore(tenant).FindPrimaryHostAsync(TenantId);

        host.ShouldBe("shop.example");
    }

    [Fact]
    public async Task Separate_database_custom_domain_stays_on_the_host_index()
    {
        var tenant = CreateTenant();
        tenant.SetDefaultConnectionString("Server=tenant-db");
        tenant.ConfigureRouting(
            "acme",
            [CreateDomain("acme.platform.test", TenantDomainType.Generated, isVerified: true, isActive: true)]);
        tenant.UpsertVerifiedCustomDomain(Guid.NewGuid(), "sabp.ir").ShouldBeTrue();
        tenant.UpsertVerifiedCustomDomain(Guid.NewGuid(), "sabp.ir").ShouldBeFalse();

        var store = CreateStore(tenant);
        (await store.FindTenantNameByHostAsync("sabp.ir")).ShouldBe("Acme");
        (await store.FindTenantNameByHostAsync("acme.platform.test")).ShouldBe("Acme");
        (await store.ListPublicHostsAsync()).Single().CustomHosts.ShouldBe(["sabp.ir"]);

        tenant.Domains.Single(domain => domain.Host == "sabp.ir").SetStatus(false, false);
        (await store.FindTenantNameByHostAsync("sabp.ir")).ShouldBeNull();
        (await store.ListPublicHostsAsync()).Single().CustomHosts.ShouldBeEmpty();
        (await store.FindTenantNameByHostAsync("acme.platform.test")).ShouldBe("Acme");

        tenant.UpsertVerifiedCustomDomain(Guid.NewGuid(), "sabp.ir").ShouldBeTrue();
        tenant.RemoveCustomDomain("acme.platform.test").ShouldBeFalse();
        tenant.RemoveCustomDomain("sabp.ir").ShouldBeTrue();
        (await store.FindTenantNameByHostAsync("sabp.ir")).ShouldBeNull();
        (await store.FindTenantNameByHostAsync("acme.platform.test")).ShouldBe("Acme");
    }

    [Fact]
    public async Task Returns_null_when_tenant_has_no_eligible_domains()
    {
        var tenant = CreateTenant();
        tenant.ConfigureRouting("acme", []);

        var host = await CreateStore(tenant).FindPrimaryHostAsync(TenantId);

        host.ShouldBeNull();
    }

    private static Tenant CreateTenant() => new(TenantId, "Acme", "ACME");

    private static TenantDomain CreateDomain(
        string host,
        TenantDomainType type,
        bool isVerified,
        bool isActive)
    {
        return new TenantDomain(Guid.NewGuid(), TenantId, host, type, isVerified, isActive);
    }

    private static TenantDomainStore CreateStore(Tenant tenant)
    {
        var repository = Substitute.For<ITenantRepository>();
        repository.GetListAsync(
                Arg.Any<string>(),
                Arg.Any<int>(),
                Arg.Any<int>(),
                Arg.Any<string>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(new List<Tenant> { tenant });

        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Change(Arg.Any<Guid?>()).Returns(Substitute.For<IDisposable>());

        var cache = Substitute.For<IDistributedCache<TenantDomainMapCacheItem>>();
        cache.GetAsync(default!, default)
            .ReturnsForAnyArgs((TenantDomainMapCacheItem?)null);

        return new TenantDomainStore(repository, currentTenant, cache);
    }
}

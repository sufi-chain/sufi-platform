using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using Volo.Abp.Authorization;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantActivationTests
{
    private static readonly Guid TenantId = Guid.Parse("21100000-0000-0000-0000-000000000211");

    [Fact]
    public async Task DeactivateTenant_TenantCannotSignIn_HostStill()
    {
        var tenant = new Tenant(TenantId, "Demo", "DEMO");
        tenant.IsActive.ShouldBeTrue();
        var harness = NewService(tenant);

        await harness.Service.DeactivateAsync(TenantId);

        tenant.IsActive.ShouldBeFalse();
        var configuration = new TenantToTenantConfigurationMapper().Map(tenant);
        configuration.IsActive.ShouldBeFalse();
        TenantSignInAllowed(configuration).ShouldBeFalse();
        TenantSignInAllowed(hostTenant: null).ShouldBeTrue();
        await harness.Cache.Received().RemoveManyAsync(
            Arg.Is<IEnumerable<string>>(keys => keys.Contains(TenantConfigurationCacheItem.CalculateCacheKey(TenantId, null))),
            Arg.Any<bool?>(),
            true,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ReactivateTenant_Works()
    {
        var tenant = new Tenant(TenantId, "Demo", "DEMO");
        tenant.Deactivate();
        var harness = NewService(tenant);

        await harness.Service.ActivateAsync(TenantId);

        tenant.IsActive.ShouldBeTrue();
        new TenantToTenantConfigurationMapper().Map(tenant).IsActive.ShouldBeTrue();
        TenantSignInAllowed(new TenantToTenantConfigurationMapper().Map(tenant)).ShouldBeTrue();
    }

    [Fact]
    public async Task TenantStore_InactiveTenant_ResolvesInactive()
    {
        var tenant = new Tenant(TenantId, "Demo", "DEMO");
        tenant.Deactivate();
        var repository = Substitute.For<ITenantRepository>();
        repository.FindAsync(TenantId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(tenant);
        var mapper = new TenantToTenantConfigurationMapper();
        var objectMapper = Substitute.For<IObjectMapper<SufiTenantsDomainModule>>();
        objectMapper.Map<Tenant, TenantConfiguration>(Arg.Any<Tenant>())
            .Returns(call => mapper.Map(call.Arg<Tenant>()));
        var current = Substitute.For<ICurrentTenant>();
        current.Change(Arg.Any<Guid?>(), Arg.Any<string?>()).Returns(Substitute.For<IDisposable>());
        var cache = Substitute.For<IDistributedCache<TenantConfigurationCacheItem>>();
        cache.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((TenantConfigurationCacheItem?)null);
        var store = new TenantStore(repository, objectMapper, current, cache);

        var resolved = await store.FindAsync(TenantId);

        resolved.ShouldNotBeNull();
        resolved.IsActive.ShouldBeFalse();
    }

    private static bool TenantSignInAllowed(TenantConfiguration? hostTenant) =>
        hostTenant == null || hostTenant.IsActive;

    private static ActivationHarness NewService(Tenant tenant)
    {
        var repository = Substitute.For<ITenantRepository>();
        repository.GetAsync(TenantId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(tenant);
        repository.UpdateAsync(Arg.Any<Tenant>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(call => call.Arg<Tenant>());
        var cache = Substitute.For<IDistributedCache<TenantConfigurationCacheItem>>();
        cache.RemoveManyAsync(
                Arg.Any<IEnumerable<string>>(),
                Arg.Any<bool?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var authorization = Substitute.For<IAbpAuthorizationService>();
        authorization.AuthorizeAsync(Arg.Any<ClaimsPrincipal>(), Arg.Any<object>(), Arg.Any<string>())
            .Returns(AuthorizationResult.Success());
        var unitOfWork = Substitute.For<IUnitOfWork>();
        var unitOfWorkManager = Substitute.For<IUnitOfWorkManager>();
        unitOfWorkManager.Current.Returns(unitOfWork);
        var services = new ServiceCollection();
        services.AddSingleton(cache);
        services.AddSingleton<IAuthorizationService>(authorization);
        services.AddSingleton(unitOfWorkManager);
        services.AddSingleton(Substitute.For<ICurrentTenant>());
        var service = new TenantAppService(
            repository,
            Substitute.For<ITenantManager>(),
            null!,
            null!,
            null!)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
        };
        return new ActivationHarness(service, cache);
    }

    private sealed record ActivationHarness(
        TenantAppService Service,
        IDistributedCache<TenantConfigurationCacheItem> Cache);
}

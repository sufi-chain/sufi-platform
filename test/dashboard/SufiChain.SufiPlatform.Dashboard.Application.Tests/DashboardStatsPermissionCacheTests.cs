using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Dashboard.Dashboard;
using SufiChain.SufiPlatform.Dashboard.Providers;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.Dashboard;

public class DashboardStatsPermissionCacheTests
{
    private const string IdentityPermission = "SufiIdentity.SecurityLogs";
    private const string HooshvarePermission = "SufiAI.Hooshvare.ViewRuns";

    [Fact]
    public async Task Plain_user_sees_only_permitted_cards_after_an_admin_warmed_the_cache()
    {
        var tenantId = Guid.NewGuid();
        var cache = new MemoryDashboardCache<DashboardStatsCacheItem>();
        var identityCalls = 0;
        var hooshvareCalls = 0;
        var identity = Provider("Identity", IdentityPermission, 30, () => identityCalls++);
        var hooshvare = Provider("AIHooshvare", HooshvarePermission, 140, () => hooshvareCalls++);

        var admin = CreateService(
            cache,
            tenantId,
            Guid.NewGuid(),
            [IdentityPermission, HooshvarePermission],
            identity,
            hooshvare);

        var warmed = await admin.GetAsync(new GetDashboardInput { Range = DashboardDateRange.Last30Days });
        warmed.Modules.Select(module => module.Module).ShouldBe(["Identity", "AIHooshvare"]);
        identityCalls.ShouldBe(1);
        hooshvareCalls.ShouldBe(1);

        var plain = CreateService(
            cache,
            tenantId,
            Guid.NewGuid(),
            [HooshvarePermission],
            identity,
            hooshvare);

        foreach (var range in Enum.GetValues<DashboardDateRange>())
        {
            var stats = await plain.GetAsync(new GetDashboardInput { Range = range });
            stats.Modules.Select(module => module.Module).ShouldBe(["AIHooshvare"]);
            stats.Modules.ShouldNotContain(module => module.Module == "Identity");
        }

        identityCalls.ShouldBe(1);
        hooshvareCalls.ShouldBe(1 + Enum.GetValues<DashboardDateRange>().Length);
    }

    [Fact]
    public async Task Provider_returns_nothing_until_permission_and_feature_are_granted()
    {
        var permissions = Substitute.For<IPermissionChecker>();
        var features = Substitute.For<IFeatureChecker>();
        permissions.IsGrantedAsync("SufiCRM.Contacts").Returns(false);
        features.IsEnabledAsync("SufiCRM.Enable").Returns(true);
        var provider = new GatedStatsProvider(permissions, features);
        var from = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = from.AddDays(30);

        (await provider.GetStatsAsync(from, to)).ShouldBeNull();
        provider.Collections.ShouldBe(0);

        permissions.IsGrantedAsync("SufiCRM.Contacts").Returns(true);
        features.IsEnabledAsync("SufiCRM.Enable").Returns(false);
        (await provider.GetStatsAsync(from, to)).ShouldBeNull();
        provider.Collections.ShouldBe(0);

        features.IsEnabledAsync("SufiCRM.Enable").Returns(true);
        var stats = await provider.GetStatsAsync(from, to);
        stats.ShouldNotBeNull();
        stats.Module.ShouldBe("CRM");
        provider.Collections.ShouldBe(1);
    }

    private static SufiDashboardAppService CreateService(
        IDistributedCache<DashboardStatsCacheItem> cache,
        Guid tenantId,
        Guid userId,
        IReadOnlyCollection<string> granted,
        params IModuleStatsProvider[] providers)
    {
        var authorization = new GrantingAuthorizationService(granted);
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        currentUser.IsAuthenticated.Returns(true);
        var currentTenant = Substitute.For<ICurrentTenant>();
        currentTenant.Id.Returns(tenantId);
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(new DateTime(2026, 11, 10, 12, 0, 0, DateTimeKind.Utc));
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(DashboardSettings.CacheMinutes).Returns("5");

        var services = new ServiceCollection();
        services.AddSingleton<IAuthorizationService>(authorization);
        services.AddSingleton(currentUser);
        services.AddSingleton(currentTenant);
        services.AddSingleton(clock);
        services.AddSingleton(settings);
        services.AddSingleton(Substitute.For<IFeatureChecker>());
        var service = new SufiDashboardAppService(providers, cache);
        service.LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        return service;
    }

    private static IModuleStatsProvider Provider(string module, string permission, int order, Action onCollect)
    {
        var provider = Substitute.For<IModuleStatsProvider>();
        provider.Module.Returns(module);
        provider.DisplayNameKey.Returns("Module:" + module);
        provider.Icon.Returns("box");
        provider.RequiredPermission.Returns(permission);
        provider.RequiredFeature.Returns((string?)null);
        provider.Order.Returns(order);
        provider.GetStatsAsync(Arg.Any<DateTime>(), Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                onCollect();
                return Task.FromResult<ModuleStatsDto?>(new ModuleStatsDto
                {
                    Module = module,
                    RequiredPermission = permission,
                    Metrics = { new MetricValueDto { Key = "Count", Value = 12 } }
                });
            });
        return provider;
    }

    private sealed class GatedStatsProvider : ModuleStatsProviderBase
    {
        public GatedStatsProvider(IPermissionChecker permissionChecker, IFeatureChecker featureChecker)
            : base(permissionChecker, featureChecker)
        {
        }

        public int Collections { get; private set; }
        public override string Module => "CRM";
        public override string DisplayNameKey => "Module:CRM";
        public override string Icon => "users";
        public override string RequiredPermission => "SufiCRM.Contacts";
        public override string? RequiredFeature => "SufiCRM.Enable";
        public override IEnumerable<StatsMetricDto> GetCatalogMetrics() => [];

        protected override Task<ModuleStatsDto> CollectStatsAsync(
            DateTime fromUtc,
            DateTime toUtc,
            CancellationToken cancellationToken = default)
        {
            Collections++;
            return Task.FromResult(NewModule());
        }
    }

    private sealed class GrantingAuthorizationService : IAbpAuthorizationService
    {
        private readonly HashSet<string> _granted;

        public GrantingAuthorizationService(IReadOnlyCollection<string> granted)
        {
            _granted = new HashSet<string>(granted, StringComparer.Ordinal);
        }

        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity("test"));

        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        public Task<AuthorizationResult> AuthorizeAsync(
            ClaimsPrincipal user,
            object? resource,
            IEnumerable<IAuthorizationRequirement> requirements)
        {
            return Task.FromResult(AuthorizationResult.Success());
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            return Task.FromResult(_granted.Contains(policyName)
                ? AuthorizationResult.Success()
                : AuthorizationResult.Failed());
        }
    }

    private sealed class MemoryDashboardCache<TCacheItem> : IDistributedCache<TCacheItem>
        where TCacheItem : class
    {
        private readonly Dictionary<string, TCacheItem> _items = new(StringComparer.Ordinal);

        public IDistributedCache<TCacheItem, string> InternalCache => this;

        public TCacheItem? Get(string key, bool? hideErrors = null, bool considerUow = false)
            => GetAsync(key, hideErrors, considerUow).GetAwaiter().GetResult();

        public Task<TCacheItem?> GetAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            _items.TryGetValue(key, out var value);
            return Task.FromResult(value);
        }

        public KeyValuePair<string, TCacheItem?>[] GetMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
            => GetManyAsync(keys, hideErrors, considerUow).GetAwaiter().GetResult();

        public Task<KeyValuePair<string, TCacheItem?>[]> GetManyAsync(
            IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            return Task.FromResult(keys.Select(key =>
            {
                _items.TryGetValue(key, out var value);
                return new KeyValuePair<string, TCacheItem?>(key, value);
            }).ToArray());
        }

        public TCacheItem? GetOrAdd(string key, Func<TCacheItem> factory, Func<DistributedCacheEntryOptions>? optionsFactory = null, bool? hideErrors = null, bool considerUow = false)
            => GetOrAddAsync(key, () => Task.FromResult(factory()), optionsFactory, hideErrors, considerUow).GetAwaiter().GetResult();

        public async Task<TCacheItem?> GetOrAddAsync(
            string key,
            Func<Task<TCacheItem>> factory,
            Func<DistributedCacheEntryOptions>? optionsFactory = null,
            bool? hideErrors = null,
            bool considerUow = false,
            CancellationToken token = default)
        {
            if (_items.TryGetValue(key, out var existing))
            {
                return existing;
            }

            var created = await factory();
            _items[key] = created;
            return created;
        }

        public KeyValuePair<string, TCacheItem?>[] GetOrAddMany(
            IEnumerable<string> keys,
            Func<IEnumerable<string>, List<KeyValuePair<string, TCacheItem>>> factory,
            Func<DistributedCacheEntryOptions>? optionsFactory = null,
            bool? hideErrors = null,
            bool considerUow = false)
            => throw new NotSupportedException();

        public Task<KeyValuePair<string, TCacheItem?>[]> GetOrAddManyAsync(
            IEnumerable<string> keys,
            Func<IEnumerable<string>, Task<List<KeyValuePair<string, TCacheItem>>>> factory,
            Func<DistributedCacheEntryOptions>? optionsFactory = null,
            bool? hideErrors = null,
            bool considerUow = false,
            CancellationToken token = default)
            => throw new NotSupportedException();

        public void Set(string key, TCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
            => _items[key] = value;

        public Task SetAsync(string key, TCacheItem value, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            _items[key] = value;
            return Task.CompletedTask;
        }

        public void SetMany(IEnumerable<KeyValuePair<string, TCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false)
        {
            foreach (var item in items)
            {
                _items[item.Key] = item.Value;
            }
        }

        public Task SetManyAsync(IEnumerable<KeyValuePair<string, TCacheItem>> items, DistributedCacheEntryOptions? options = null, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            SetMany(items, options, hideErrors, considerUow);
            return Task.CompletedTask;
        }

        public void Refresh(string key, bool? hideErrors = null)
        {
        }

        public Task RefreshAsync(string key, bool? hideErrors = null, CancellationToken token = default) => Task.CompletedTask;

        public void RefreshMany(IEnumerable<string> keys, bool? hideErrors = null)
        {
        }

        public Task RefreshManyAsync(IEnumerable<string> keys, bool? hideErrors = null, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key, bool? hideErrors = null, bool considerUow = false) => _items.Remove(key);

        public Task RemoveAsync(string key, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            _items.Remove(key);
            return Task.CompletedTask;
        }

        public void RemoveMany(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false)
        {
            foreach (var key in keys)
            {
                _items.Remove(key);
            }
        }

        public Task RemoveManyAsync(IEnumerable<string> keys, bool? hideErrors = null, bool considerUow = false, CancellationToken token = default)
        {
            RemoveMany(keys, hideErrors, considerUow);
            return Task.CompletedTask;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomainStore : ITenantDomainStore, ITransientDependency
{
    protected ITenantRepository TenantRepository { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IDistributedCache<TenantDomainMapCacheItem> Cache { get; }

    public TenantDomainStore(
        ITenantRepository tenantRepository,
        ICurrentTenant currentTenant,
        IDistributedCache<TenantDomainMapCacheItem> cache)
    {
        TenantRepository = tenantRepository;
        CurrentTenant = currentTenant;
        Cache = cache;
    }

    public virtual async Task<string?> FindTenantNameByHostAsync(string host)
    {
        var normalizedHost = TenantDomainName.NormalizeHost(host);
        var cacheItem = await GetOrBuildCacheItemAsync();

        return cacheItem.HostToTenantName.TryGetValue(normalizedHost, out var tenantName)
            ? tenantName
            : null;
    }

    public virtual async Task<string?> FindPrimaryHostAsync(Guid tenantId)
    {
        var cacheItem = await GetOrBuildCacheItemAsync();
        return cacheItem.TenantIdToPrimaryHost.TryGetValue(tenantId, out var host)
            ? host
            : null;
    }

    protected virtual async Task<TenantDomainMapCacheItem> GetOrBuildCacheItemAsync()
    {
        var cacheItem = await Cache.GetAsync(TenantDomainMapCacheItem.CacheKey, considerUow: true);
        if (cacheItem == null)
        {
            cacheItem = await BuildCacheItemAsync();
            await Cache.SetAsync(TenantDomainMapCacheItem.CacheKey, cacheItem, considerUow: true);
        }

        return cacheItem;
    }

    protected virtual async Task<TenantDomainMapCacheItem> BuildCacheItemAsync()
    {
        List<Tenant> tenants;
        using (CurrentTenant.Change(null))
        {
            tenants = await TenantRepository.GetListAsync(includeDetails: true);
        }

        var hostToTenantName = tenants
            .SelectMany(tenant => tenant.Domains
                .Where(domain => domain.IsActive && domain.IsVerified)
                .Select(domain => new { domain.Host, tenant.Name }))
            .ToDictionary(
                entry => entry.Host,
                entry => entry.Name,
                StringComparer.OrdinalIgnoreCase);

        var tenantIdToPrimaryHost = new Dictionary<Guid, string>();
        foreach (var tenant in tenants)
        {
            var primary = tenant.Domains
                .Where(domain => domain.IsActive && domain.IsVerified)
                .OrderBy(domain => domain.Type == TenantDomainType.Custom ? 0 : 1)
                .ThenBy(domain => domain.CreationTime)
                .Select(domain => domain.Host)
                .FirstOrDefault();
            if (!primary.IsNullOrWhiteSpace())
            {
                tenantIdToPrimaryHost[tenant.Id] = primary!;
            }
        }

        return new TenantDomainMapCacheItem
        {
            HostToTenantName = hostToTenantName,
            TenantIdToPrimaryHost = tenantIdToPrimaryHost
        };
    }
}

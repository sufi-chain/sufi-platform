using System;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;
using Volo.Abp.EventBus.Local;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomainMapCacheInvalidator :
    ILocalEventHandler<EntityChangedEventData<Tenant>>,
    IDistributedEventHandler<TenantDomainMapChangedEto>,
    ITransientDependency
{
    protected IDistributedCache<TenantDomainMapCacheItem> Cache { get; }
    protected IDistributedEventBus DistributedEventBus { get; }

    public TenantDomainMapCacheInvalidator(
        IDistributedCache<TenantDomainMapCacheItem> cache,
        IDistributedEventBus distributedEventBus)
    {
        Cache = cache;
        DistributedEventBus = distributedEventBus;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<Tenant> eventData)
    {
        await Cache.RemoveAsync(TenantDomainMapCacheItem.CacheKey, considerUow: true);
        await DistributedEventBus.PublishAsync(new TenantDomainMapChangedEto
        {
            TenantId = eventData.Entity?.Id,
            OccurredAt = DateTime.UtcNow
        });
    }

    public virtual Task HandleEventAsync(TenantDomainMapChangedEto eventData)
    {
        return Cache.RemoveAsync(TenantDomainMapCacheItem.CacheKey, considerUow: false);
    }
}

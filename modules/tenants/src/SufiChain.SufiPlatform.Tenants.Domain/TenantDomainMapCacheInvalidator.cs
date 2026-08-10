using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Local;

namespace SufiChain.SufiPlatform.Tenants;

public class TenantDomainMapCacheInvalidator :
    ILocalEventHandler<EntityChangedEventData<Tenant>>,
    ITransientDependency
{
    protected IDistributedCache<TenantDomainMapCacheItem> Cache { get; }

    public TenantDomainMapCacheInvalidator(IDistributedCache<TenantDomainMapCacheItem> cache)
    {
        Cache = cache;
    }

    public virtual Task HandleEventAsync(EntityChangedEventData<Tenant> eventData)
    {
        return Cache.RemoveAsync(TenantDomainMapCacheItem.CacheKey, considerUow: true);
    }
}

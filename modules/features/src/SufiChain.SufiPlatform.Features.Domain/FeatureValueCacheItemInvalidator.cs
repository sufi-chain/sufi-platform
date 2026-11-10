using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;
using Volo.Abp.EventBus.Distributed;

namespace SufiChain.SufiPlatform.Features;

public class FeatureValueCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<FeatureValue>>,
    IDistributedEventHandler<FeatureValueChangedEto>,
    ITransientDependency
{
    protected IDistributedCache<FeatureValueCacheItem> Cache { get; }

    protected IDistributedEventBus DistributedEventBus { get; }

    public FeatureValueCacheItemInvalidator(
        IDistributedCache<FeatureValueCacheItem> cache,
        IDistributedEventBus distributedEventBus)
    {
        Cache = cache;
        DistributedEventBus = distributedEventBus;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<FeatureValue> eventData)
    {
        var name = eventData.Entity.Name;
        var providerName = eventData.Entity.ProviderName;
        var providerKey = eventData.Entity.ProviderKey;

        await RemoveAsync(name, providerName, providerKey);

        await DistributedEventBus.PublishAsync(new FeatureValueChangedEto
        {
            Name = name,
            ProviderName = providerName,
            ProviderKey = providerKey
        });
    }

    public virtual Task HandleEventAsync(FeatureValueChangedEto eventData)
    {
        return RemoveAsync(eventData.Name, eventData.ProviderName, eventData.ProviderKey);
    }

    protected virtual async Task RemoveAsync(string name, string providerName, string? providerKey)
    {
        foreach (var cacheKey in EnumerateCacheKeys(name, providerName, providerKey))
        {
            await Cache.RemoveAsync(cacheKey, considerUow: false);
            await Cache.RemoveAsync(cacheKey, considerUow: true);
        }
    }

    protected virtual IEnumerable<string> EnumerateCacheKeys(string name, string providerName, string? providerKey)
    {
        var alternateProviderKey = providerKey switch
        {
            null => string.Empty,
            { Length: 0 } => null,
            _ => providerKey
        };

        return new[]
        {
            CalculateCacheKey(name, providerName, providerKey),
            CalculateCacheKey(name, providerName, alternateProviderKey)
        }.Distinct();
    }

    protected virtual string CalculateCacheKey(string name, string providerName, string? providerKey)
    {
        return FeatureValueCacheItem.CalculateCacheKey(name, providerName, providerKey);
    }
}

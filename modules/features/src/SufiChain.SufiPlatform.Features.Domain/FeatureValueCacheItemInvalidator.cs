using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Features;

public class FeatureValueCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<FeatureValue>>,
    ITransientDependency
{
    protected IDistributedCache<FeatureValueCacheItem> Cache { get; }

    public FeatureValueCacheItemInvalidator(IDistributedCache<FeatureValueCacheItem> cache)
    {
        Cache = cache;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<FeatureValue> eventData)
    {
        foreach (var cacheKey in EnumerateCacheKeys(
                     eventData.Entity.Name,
                     eventData.Entity.ProviderName,
                     eventData.Entity.ProviderKey))
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

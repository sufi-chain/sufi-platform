using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Tags.Caching;

/// <summary>
/// Invalidates tag lookups whenever a <see cref="Tag"/> is created, updated,
/// or deleted. Tags are referenced by scope, so we drop the per-scope list cache.
/// </summary>
public class TagCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<Tag>>,
    ITransientDependency
{
    private readonly IDistributedCache<TagCacheItem> _cache;

    public TagCacheItemInvalidator(IDistributedCache<TagCacheItem> cache)
    {
        _cache = cache;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<Tag> eventData)
    {
        await _cache.RemoveAsync(
            TagCacheItem.CreateScopeListCacheKey(eventData.Entity.Scope),
            considerUow: true);
    }
}

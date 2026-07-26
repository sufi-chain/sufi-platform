using SufiChain.SufiPlatform.Tags.Tags;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Tags.Caching;

/// <summary>
/// Invalidates the per-entity tag cache whenever a <see cref="TagLink"/> is
/// assigned or unassigned (covers all POST/DELETE paths).
/// </summary>
public class TagLinkCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<TagLink>>,
    ITransientDependency
{
    private readonly IDistributedCache<TagLinkCacheItem> _cache;

    public TagLinkCacheItemInvalidator(IDistributedCache<TagLinkCacheItem> cache)
    {
        _cache = cache;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<TagLink> eventData)
    {
        var link = eventData.Entity;
        await _cache.RemoveAsync(
            TagLinkCacheItem.CreateEntityTagsCacheKey(link.EntityType, link.EntityId),
            considerUow: true);
    }
}

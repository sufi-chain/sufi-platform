using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// Invalidates menu item tree caches whenever a <see cref="MenuItem"/> is
/// created, updated, moved, reordered, or deleted (covers all POST/PUT/DELETE paths).
/// Public item-by-slug keys are removed as well to avoid stale slug lookups.
/// </summary>
public class MenuItemCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<MenuItem>>,
    ITransientDependency
{
    private readonly IDistributedCache<MenuTreeCacheItem> _treeCache;

    public MenuItemCacheItemInvalidator(IDistributedCache<MenuTreeCacheItem> treeCache)
    {
        _treeCache = treeCache;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<MenuItem> eventData)
    {
        var item = eventData.Entity;

        await _treeCache.RemoveAsync(
            MenuTreeCacheItem.CreateTreeCacheKey(item.MenuId, publicOnly: false),
            considerUow: true);

        await _treeCache.RemoveAsync(
            MenuTreeCacheItem.CreateTreeCacheKey(item.MenuId, publicOnly: true),
            considerUow: true);
    }
}

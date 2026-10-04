using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// Invalidates menu item tree caches whenever a <see cref="MenuItem"/> is
/// created, updated, moved, reordered, or deleted (covers all POST/PUT/DELETE paths).
/// Admin <c>t:</c> keys and the public <c>pt:</c> / <c>pi:</c> keys for this item's menu are removed.
/// </summary>
public class MenuItemCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<MenuItem>>,
    ITransientDependency
{
    private readonly IDistributedCache<MenuTreeCacheItem> _treeCache;
    private readonly IMenuRepository _menuRepository;

    public MenuItemCacheItemInvalidator(
        IDistributedCache<MenuTreeCacheItem> treeCache,
        IMenuRepository menuRepository)
    {
        _treeCache = treeCache;
        _menuRepository = menuRepository;
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

        var menu = await _menuRepository.FindAsync(item.MenuId);
        if (menu == null)
        {
            return;
        }

        foreach (var cacheKey in MenuTreeCacheKeys.CreatePublicInvalidationKeys(
                     menu.ContextType,
                     menu.ContextId,
                     menu.Name,
                     item.Slug))
        {
            await _treeCache.RemoveAsync(cacheKey, considerUow: true);
        }
    }
}

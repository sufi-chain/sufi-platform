using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// Invalidates menu and menu tree caches whenever a <see cref="Menu"/> aggregate
/// is created, updated, or deleted (covers all POST/PUT/DELETE paths).
/// </summary>
public class MenuCacheItemInvalidator :
    ILocalEventHandler<EntityChangedEventData<Menu>>,
    ITransientDependency
{
    private readonly IDistributedCache<MenuCacheItem> _menuCache;
    private readonly IDistributedCache<MenuTreeCacheItem> _treeCache;

    public MenuCacheItemInvalidator(
        IDistributedCache<MenuCacheItem> menuCache,
        IDistributedCache<MenuTreeCacheItem> treeCache)
    {
        _menuCache = menuCache;
        _treeCache = treeCache;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<Menu> eventData)
    {
        var menu = eventData.Entity;

        await _menuCache.RemoveAsync(
            MenuCacheItem.CreateCacheKey(menu.ContextType, menu.ContextId, menu.Name),
            considerUow: true);

        await _treeCache.RemoveAsync(
            MenuTreeCacheItem.CreatePublicTreeCacheKey(menu.ContextType, menu.ContextId, menu.Name),
            considerUow: true);

        await _treeCache.RemoveAsync(
            MenuTreeCacheItem.CreateTreeCacheKey(menu.Id, publicOnly: false),
            considerUow: true);

        await _treeCache.RemoveAsync(
            MenuTreeCacheItem.CreateTreeCacheKey(menu.Id, publicOnly: true),
            considerUow: true);
    }
}

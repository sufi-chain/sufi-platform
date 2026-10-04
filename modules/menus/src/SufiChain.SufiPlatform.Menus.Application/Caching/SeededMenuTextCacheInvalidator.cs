using SufiChain.SufiPlatform.Localization.Entities;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace SufiChain.SufiPlatform.Menus.Caching;

/// <summary>
/// A Localization → Texts edit of a <c>SeededMenu:</c> key evicts the public menu cache
/// for every culture, because the cached tree is unresolved.
/// </summary>
public class SeededMenuTextCacheInvalidator :
    ILocalEventHandler<EntityChangedEventData<LocalizationText>>,
    ITransientDependency
{
    private readonly IDistributedCache<MenuTreeCacheItem> _treeCache;
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;

    public SeededMenuTextCacheInvalidator(
        IDistributedCache<MenuTreeCacheItem> treeCache,
        IMenuRepository menuRepository,
        IMenuItemRepository menuItemRepository)
    {
        _treeCache = treeCache;
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
    }

    public virtual async Task HandleEventAsync(EntityChangedEventData<LocalizationText> eventData)
    {
        var key = eventData.Entity.Key;
        if (string.IsNullOrWhiteSpace(key) || !key.StartsWith("SeededMenu:", StringComparison.Ordinal))
        {
            return;
        }

        var menus = await _menuRepository.GetListAsync();
        foreach (var menu in menus)
        {
            if (string.Equals(menu.DisplayName, key, StringComparison.Ordinal))
            {
                await EvictAsync(menu, slug: null);
            }

            var items = await _menuItemRepository.GetTreeItemsAsync(menu.Id, menu.TenantId);
            foreach (var item in items)
            {
                if (string.Equals(item.DisplayName, key, StringComparison.Ordinal))
                {
                    await EvictAsync(menu, item.Slug);
                }
            }
        }
    }

    private async Task EvictAsync(Menu menu, string? slug)
    {
        foreach (var cacheKey in MenuTreeCacheKeys.CreatePublicInvalidationKeys(
                     menu.ContextType,
                     menu.ContextId,
                     menu.Name,
                     slug))
        {
            await _treeCache.RemoveAsync(cacheKey, considerUow: true);
        }
    }
}

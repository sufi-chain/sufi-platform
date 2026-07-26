using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Menus.Caching;
using SufiChain.SufiPlatform.Menus.Features;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.Caching;

namespace SufiChain.SufiPlatform.Menus.Menus;

[RequiresFeature(SufiMenusFeatures.Enable, SufiMenusFeatures.PublicMenus)]
[AllowAnonymous]
public class PublicMenuAppService : SufiApplicationService, IPublicMenuAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly MenuBusinessLocalizationService _businessLocalization;
    private readonly IDistributedCache<MenuTreeCacheItem> _treeCache;

    public PublicMenuAppService(
        IMenuRepository menuRepository,
        IMenuItemRepository menuItemRepository,
        MenuBusinessLocalizationService businessLocalization,
        IDistributedCache<MenuTreeCacheItem> treeCache)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _businessLocalization = businessLocalization;
        _treeCache = treeCache;
    }

    public virtual async Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName)
    {
        var cacheKey = MenuTreeCacheItem.CreatePublicTreeCacheKey(contextType, contextId, menuName);
        var cached = await _treeCache.GetOrAddAsync(cacheKey, async () =>
        {
            var menu = await _menuRepository.FindByNameAsync(contextType, contextId, menuName, CurrentTenant.Id, includeDetails: false);
            if (menu == null || !menu.IsActive)
            {
                return new MenuTreeCacheItem();
            }

            var items = (await _menuItemRepository.GetTreeItemsAsync(menu.Id, CurrentTenant.Id))
                .Where(x => x.IsActive && x.IsVisible)
                .ToList();

            return new MenuTreeCacheItem { Tree = BuildTree(items, null, menu.ContextType) };
        });

        return cached.Tree;
    }

    public virtual async Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug)
    {
        var cacheKey = MenuTreeCacheItem.CreatePublicItemCacheKey(contextType, contextId, menuName, slug);
        var cached = await _treeCache.GetOrAddAsync(cacheKey, async () =>
        {
            var menu = await _menuRepository.FindByNameAsync(contextType, contextId, menuName, CurrentTenant.Id, includeDetails: false);
            if (menu == null || !menu.IsActive)
            {
                return new MenuTreeCacheItem();
            }

            var item = await _menuItemRepository.FindBySlugAsync(menu.Id, slug, CurrentTenant.Id, includeDetails: false);
            if (item is not { IsActive: true, IsVisible: true })
            {
                return new MenuTreeCacheItem();
            }

            var dto = item.ToDto();
            dto.DisplayName = _businessLocalization.ResolveMenuItemDisplayName(dto.DisplayName, menu.ContextType);
            return new MenuTreeCacheItem { Item = dto };
        });

        return cached.Item;
    }

    protected virtual List<MenuItemTreeDto> BuildTree(List<MenuItem> items, Guid? parentId, string contextType) =>
        items
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.DisplayName)
            .Select(x =>
            {
                var dto = x.ToTreeDto();
                dto.DisplayName = _businessLocalization.ResolveMenuItemDisplayName(dto.DisplayName, contextType);
                dto.Children = BuildTree(items, x.Id, contextType);
                return dto;
            })
            .ToList();
}

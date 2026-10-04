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

            return new MenuTreeCacheItem { Tree = BuildTree(items, null) };
        });

        return LocalizeTree(cached.Tree, contextType);
    }

    public virtual async Task<List<MenuItemTreeDto>?> GetTreeByIdAsync(Guid menuId)
    {
        var menu = await _menuRepository.FindAsync(menuId);
        if (menu == null || !menu.IsActive)
        {
            return null;
        }

        return await GetTreeAsync(menu.ContextType, menu.ContextId, menu.Name);
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

            return new MenuTreeCacheItem { Item = item.ToDto() };
        });

        return cached.Item == null ? null : LocalizeItem(cached.Item, contextType);
    }

    protected virtual List<MenuItemTreeDto> BuildTree(List<MenuItem> items, Guid? parentId) =>
        items
            .Where(x => x.ParentId == parentId)
            .OrderBy(x => x.DisplayOrder)
            .ThenBy(x => x.DisplayName)
            .Select(x =>
            {
                var dto = x.ToTreeDto();
                dto.Children = BuildTree(items, x.Id);
                return dto;
            })
            .ToList();

    protected virtual List<MenuItemTreeDto> LocalizeTree(List<MenuItemTreeDto> items, string contextType)
    {
        var localized = new List<MenuItemTreeDto>(items.Count);
        foreach (var item in items)
        {
            var copy = CopyTreeItem(item);
            copy.DisplayName = _businessLocalization.ResolveMenuItemDisplayName(item.DisplayName, contextType, item.Name);
            copy.Children = LocalizeTree(item.Children, contextType);
            localized.Add(copy);
        }

        return localized;
    }

    protected virtual MenuItemDto LocalizeItem(MenuItemDto item, string contextType)
    {
        var copy = CopyItem(item);
        copy.DisplayName = _businessLocalization.ResolveMenuItemDisplayName(item.DisplayName, contextType, item.Name);
        return copy;
    }

    private static MenuItemTreeDto CopyTreeItem(MenuItemTreeDto source)
    {
        var copy = new MenuItemTreeDto();
        CopyItem(source, copy);
        return copy;
    }

    private static MenuItemDto CopyItem(MenuItemDto source)
    {
        var copy = new MenuItemDto();
        CopyItem(source, copy);
        return copy;
    }

    private static void CopyItem(MenuItemDto source, MenuItemDto target)
    {
        target.Id = source.Id;
        target.TenantId = source.TenantId;
        target.MenuId = source.MenuId;
        target.ParentId = source.ParentId;
        target.Name = source.Name;
        target.DisplayName = source.DisplayName;
        target.Slug = source.Slug;
        target.Description = source.Description;
        target.DisplayOrder = source.DisplayOrder;
        target.Kind = source.Kind;
        target.DisplayType = source.DisplayType;
        target.Url = source.Url;
        target.LinkTarget = source.LinkTarget;
        target.TargetType = source.TargetType;
        target.TargetId = source.TargetId;
        target.Icon = source.Icon;
        target.CssClass = source.CssClass;
        target.PermissionName = source.PermissionName;
        target.ComponentName = source.ComponentName;
        target.IsActive = source.IsActive;
        target.IsVisible = source.IsVisible;
        target.CreationTime = source.CreationTime;
        target.CreatorId = source.CreatorId;
        target.LastModificationTime = source.LastModificationTime;
        target.LastModifierId = source.LastModifierId;
        target.IsDeleted = source.IsDeleted;
        target.DeleterId = source.DeleterId;
        target.DeletionTime = source.DeletionTime;
    }
}

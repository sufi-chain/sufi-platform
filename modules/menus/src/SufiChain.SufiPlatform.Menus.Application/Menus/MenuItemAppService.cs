using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Menus.Caching;
using SufiChain.SufiPlatform.Menus.Features;
using SufiChain.SufiPlatform.Menus.Permissions;
using Volo.Abp;
using Volo.Abp.Caching;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Menus.Menus;

[RequiresFeature(SufiMenusFeatures.Enable, SufiMenusFeatures.Menus)]
[Authorize(MenusPermissions.Menus.Default)]
public class MenuItemAppService : SufiApplicationService, IMenuItemAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly MenuManager _menuManager;
    private readonly IDistributedCache<MenuTreeCacheItem> _treeCache;
    private readonly IDistributedCache<MenuCacheItem> _menuCache;
    private readonly MenuLabelLocalization _labels;

    public MenuItemAppService(
        IMenuRepository menuRepository,
        IMenuItemRepository menuItemRepository,
        MenuManager menuManager,
        IDistributedCache<MenuTreeCacheItem> treeCache,
        IDistributedCache<MenuCacheItem> menuCache,
        MenuLabelLocalization labels)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _menuManager = menuManager;
        _treeCache = treeCache;
        _menuCache = menuCache;
        _labels = labels;
    }

    public virtual async Task<MenuItemDto> GetAsync(Guid id)
    {
        var item = await _menuItemRepository.GetAsync(id);
        var menu = await _menuRepository.FindAsync(item.MenuId);
        return await ToLabeledDtoAsync(item, menu?.ContextType);
    }

    public virtual async Task<PagedResultDto<MenuItemDto>> GetListAsync(GetMenuItemsInput input)
    {
        var items = await _menuItemRepository.GetTreeItemsAsync(input.MenuId, CurrentTenant.Id);
        var query = ApplyFilters(items.AsEnumerable(), input);
        query = ApplySorting(query, input.Sorting);
        var total = query.LongCount();
        var result = query.Skip(input.SkipCount).Take(input.MaxResultCount).Select(x => x.ToDto()).ToList();
        return new PagedResultDto<MenuItemDto>(total, result);
    }

    public virtual async Task<List<MenuItemTreeDto>> GetTreeAsync(GetMenuTreeInput input)
    {
        var menuId = await ResolveMenuIdAsync(input);
        var cacheKey = MenuTreeCacheItem.CreateTreeCacheKey(menuId, input.PublicOnly);
        var cached = await _treeCache.GetOrAddAsync(cacheKey, async () =>
        {
            var items = await _menuItemRepository.GetTreeItemsAsync(menuId, CurrentTenant.Id);
            var query = input.PublicOnly ? items.Where(x => x.IsActive && x.IsVisible).ToList() : items;
            return new MenuTreeCacheItem { Tree = BuildTree(query, null) };
        });

        return cached.Tree;
    }

    public virtual async Task<MenuItemDto?> FindBySlugAsync(Guid menuId, string slug)
    {
        var cacheKey = MenuTreeCacheItem.CreateTreeCacheKey(menuId, publicOnly: false) + $":slug:{slug}";
        var cached = await _treeCache.GetOrAddAsync(cacheKey, async () =>
        {
            var item = await _menuItemRepository.FindBySlugAsync(menuId, slug, CurrentTenant.Id);
            return new MenuTreeCacheItem { Item = item?.ToDto() };
        });

        return cached.Item;
    }

    [Authorize(MenusPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> CreateAsync(CreateMenuItemDto input)
    {
        var provisional = input.DisplayNames != null || string.IsNullOrWhiteSpace(input.DisplayName)
            ? input.Name
            : input.DisplayName;
        var item = await _menuManager.CreateItemAsync(input.MenuId, input.Name, provisional, input.Slug, input.ParentId, CurrentTenant.Id);
        ApplyInput(item, input);
        if (input.DisplayNames != null)
        {
            await _labels.StoreAsync(
                existingStored: null,
                MenuDisplayNamePlanner.ItemKey(input.MenuId, item.Id),
                input.DisplayNames,
                item.SetDisplayName,
                (await _menuRepository.FindAsync(input.MenuId))?.ContextType);
        }

        await _menuManager.ValidateItemAsync(item);
        await _menuItemRepository.InsertAsync(item, autoSave: true);
        var menu = await _menuRepository.FindAsync(item.MenuId);
        return await ToLabeledDtoAsync(item, menu?.ContextType);
    }

    [Authorize(MenusPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto input)
    {
        var item = await _menuItemRepository.GetAsync(id);
        item.SetName(input.Name);
        if (input.DisplayNames != null)
        {
            await _labels.StoreAsync(
                item.DisplayName,
                MenuDisplayNamePlanner.ItemKey(item.MenuId, item.Id),
                input.DisplayNames,
                item.SetDisplayName,
                (await _menuRepository.FindAsync(item.MenuId))?.ContextType);
        }
        else
        {
            item.SetDisplayName(DisplayNameOrName(input.DisplayName, input.Name));
        }

        if (!string.IsNullOrWhiteSpace(input.Slug) && !string.Equals(input.Slug, item.Slug, StringComparison.OrdinalIgnoreCase)) await _menuManager.ChangeItemSlugAsync(item, input.Slug);
        await _menuManager.MoveItemAsync(item, input.ParentId, input.DisplayOrder);
        ApplyInput(item, input);
        await _menuManager.ValidateItemAsync(item);
        await _menuItemRepository.UpdateAsync(item, autoSave: true);
        var menu = await _menuRepository.FindAsync(item.MenuId);
        return await ToLabeledDtoAsync(item, menu?.ContextType);
    }

    [Authorize(MenusPermissions.Menus.ManageItems)]
    public virtual async Task DeleteAsync(Guid id) => await _menuItemRepository.DeleteAsync(id, autoSave: true);

    [Authorize(MenusPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> MoveAsync(Guid id, MoveMenuItemDto input)
    {
        var item = await _menuItemRepository.GetAsync(id);
        await _menuManager.MoveItemAsync(item, input.ParentId, input.DisplayOrder);
        await _menuItemRepository.UpdateAsync(item, autoSave: true);
        return item.ToDto();
    }

    [Authorize(MenusPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> ReorderAsync(Guid id, int displayOrder)
    {
        var item = await _menuItemRepository.GetAsync(id);
        item.Reorder(displayOrder);
        await _menuItemRepository.UpdateAsync(item, autoSave: true);
        return item.ToDto();
    }

    protected virtual async Task<MenuItemDto> ToLabeledDtoAsync(MenuItem item, string? contextType)
    {
        var dto = item.ToDto();
        var labels = await _labels.ReadAsync(item.DisplayName, item.Name, contextType);
        dto.DisplayNames = labels.Values;
        dto.DisplayNameBases = labels.BaseValues;
        return dto;
    }

    protected virtual string DisplayNameOrName(string? displayName, string name)
    {
        if (string.IsNullOrWhiteSpace(displayName) || BusinessTextEditorStorage.IsPlaceholder(displayName))
        {
            return string.IsNullOrWhiteSpace(name) ? displayName ?? string.Empty : name;
        }

        return displayName;
    }

    protected virtual void ApplyInput(MenuItem item, CreateMenuItemDto input)
    {
        item.SetDescription(input.Description); item.Reorder(input.DisplayOrder); item.SetKind(input.Kind); item.SetDisplayType(input.DisplayType); item.SetLink(input.Url, input.LinkTarget); item.SetTarget(input.TargetType, input.TargetId); item.SetIcon(input.Icon); item.SetCssClass(input.CssClass); item.SetPermissionName(input.PermissionName); item.SetComponentName(input.ComponentName); if (input.IsActive) item.Activate(); else item.Deactivate(); if (input.IsVisible) item.Show(); else item.Hide();
        ApplyCultureUrls(item, input.CultureUrls);
    }

    protected virtual void ApplyInput(MenuItem item, UpdateMenuItemDto input)
    {
        item.SetDescription(input.Description); item.SetKind(input.Kind); item.SetDisplayType(input.DisplayType); item.SetLink(input.Url, input.LinkTarget); item.SetTarget(input.TargetType, input.TargetId); item.SetIcon(input.Icon); item.SetCssClass(input.CssClass); item.SetPermissionName(input.PermissionName); item.SetComponentName(input.ComponentName); if (input.IsActive) item.Activate(); else item.Deactivate(); if (input.IsVisible) item.Show(); else item.Hide();
        ApplyCultureUrls(item, input.CultureUrls);
    }

    /// <summary>Null leaves stored overrides unchanged so older update callers do not wipe them.</summary>
    private static void ApplyCultureUrls(MenuItem item, IReadOnlyDictionary<string, string>? cultureUrls)
    {
        if (cultureUrls != null)
        {
            item.SetCultureUrls(cultureUrls);
        }
    }

    protected virtual IEnumerable<MenuItem> ApplyFilters(IEnumerable<MenuItem> query, GetMenuItemsInput input)
    {
        if (input.ParentId.HasValue) query = query.Where(x => x.ParentId == input.ParentId.Value);
        if (!string.IsNullOrWhiteSpace(input.Keyword)) query = query.Where(x => x.DisplayName.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) || x.Slug.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase));
        if (input.Kind.HasValue) query = query.Where(x => x.Kind == input.Kind.Value);
        if (input.DisplayType.HasValue) query = query.Where(x => x.DisplayType == input.DisplayType.Value);
        if (!string.IsNullOrWhiteSpace(input.TargetType)) query = query.Where(x => x.TargetType == input.TargetType);
        if (input.TargetId.HasValue) query = query.Where(x => x.TargetId == input.TargetId.Value);
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        if (input.IsVisible.HasValue) query = query.Where(x => x.IsVisible == input.IsVisible.Value);
        return query;
    }

    protected virtual IEnumerable<MenuItem> ApplySorting(IEnumerable<MenuItem> query, string? sorting) => sorting?.Trim().ToLowerInvariant() switch
    {
        "displayorder desc" => query.OrderByDescending(x => x.DisplayOrder),
        "displayname" => query.OrderBy(x => x.DisplayName),
        "displayname desc" => query.OrderByDescending(x => x.DisplayName),
        _ => query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName)
    };

    protected virtual List<MenuItemTreeDto> BuildTree(List<MenuItem> items, Guid? parentId) => items.Where(x => x.ParentId == parentId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName).Select(x => { var dto = x.ToTreeDto(); dto.Children = BuildTree(items, x.Id); return dto; }).ToList();

    protected virtual async Task<Guid> ResolveMenuIdAsync(GetMenuTreeInput input)
    {
        if (input.MenuId.HasValue) return input.MenuId.Value;
        if (string.IsNullOrWhiteSpace(input.ContextType) || string.IsNullOrWhiteSpace(input.MenuName)) throw new BusinessException(MenusErrorCodes.MenuNotFound);

        var cacheKey = MenuCacheItem.CreateCacheKey(input.ContextType, input.ContextId, input.MenuName);
        var cached = await _menuCache.GetOrAddAsync(cacheKey, async () =>
        {
            var menu = await _menuRepository.FindByNameAsync(input.ContextType, input.ContextId, input.MenuName, CurrentTenant.Id, includeDetails: false)
                ?? throw new BusinessException(MenusErrorCodes.MenuNotFound).WithData("Name", input.MenuName);
            return new MenuCacheItem { Menu = menu };
        });

        return cached.Menu!.Id;
    }
}

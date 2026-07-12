using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.MenuManagement.Features;
using SufiChain.SufiAbp.MenuManagement.Permissions;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Services;
using SufiChain.SufiAbp.Features;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

[RequiresFeature(SufiAbpMenuManagementFeatures.Enable, SufiAbpMenuManagementFeatures.Menus)]
[Authorize(MenuManagementPermissions.Menus.Default)]
public class MenuItemAppService : SufiAbpApplicationService, IMenuItemAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly MenuManager _menuManager;

    public MenuItemAppService(IMenuRepository menuRepository, IMenuItemRepository menuItemRepository, MenuManager menuManager)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _menuManager = menuManager;
    }

    public virtual async Task<MenuItemDto> GetAsync(Guid id) => (await _menuItemRepository.GetAsync(id)).ToDto();

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
        var items = await _menuItemRepository.GetTreeItemsAsync(menuId, CurrentTenant.Id);
        var query = input.PublicOnly ? items.Where(x => x.IsActive && x.IsVisible).ToList() : items;
        return BuildTree(query, null);
    }

    public virtual async Task<MenuItemDto?> FindBySlugAsync(Guid menuId, string slug)
    {
        return (await _menuItemRepository.FindBySlugAsync(menuId, slug, CurrentTenant.Id))?.ToDto();
    }

    [Authorize(MenuManagementPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> CreateAsync(CreateMenuItemDto input)
    {
        var item = await _menuManager.CreateItemAsync(input.MenuId, input.Name, input.DisplayName, input.Slug, input.ParentId, CurrentTenant.Id);
        ApplyInput(item, input);
        await _menuManager.ValidateItemAsync(item);
        await _menuItemRepository.InsertAsync(item, autoSave: true);
        return item.ToDto();
    }

    [Authorize(MenuManagementPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> UpdateAsync(Guid id, UpdateMenuItemDto input)
    {
        var item = await _menuItemRepository.GetAsync(id);
        item.SetName(input.Name);
        item.SetDisplayName(input.DisplayName);
        if (!string.IsNullOrWhiteSpace(input.Slug) && !string.Equals(input.Slug, item.Slug, StringComparison.OrdinalIgnoreCase)) await _menuManager.ChangeItemSlugAsync(item, input.Slug);
        await _menuManager.MoveItemAsync(item, input.ParentId, input.DisplayOrder);
        ApplyInput(item, input);
        await _menuManager.ValidateItemAsync(item);
        await _menuItemRepository.UpdateAsync(item, autoSave: true);
        return item.ToDto();
    }

    [Authorize(MenuManagementPermissions.Menus.ManageItems)]
    public virtual async Task DeleteAsync(Guid id) => await _menuItemRepository.DeleteAsync(id, autoSave: true);

    [Authorize(MenuManagementPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> MoveAsync(Guid id, MoveMenuItemDto input)
    {
        var item = await _menuItemRepository.GetAsync(id);
        await _menuManager.MoveItemAsync(item, input.ParentId, input.DisplayOrder);
        await _menuItemRepository.UpdateAsync(item, autoSave: true);
        return item.ToDto();
    }

    [Authorize(MenuManagementPermissions.Menus.ManageItems)]
    public virtual async Task<MenuItemDto> ReorderAsync(Guid id, int displayOrder)
    {
        var item = await _menuItemRepository.GetAsync(id);
        item.Reorder(displayOrder);
        await _menuItemRepository.UpdateAsync(item, autoSave: true);
        return item.ToDto();
    }

    protected virtual void ApplyInput(MenuItem item, CreateMenuItemDto input)
    {
        item.SetDescription(input.Description); item.Reorder(input.DisplayOrder); item.SetKind(input.Kind); item.SetDisplayType(input.DisplayType); item.SetLink(input.Url, input.LinkTarget); item.SetTarget(input.TargetType, input.TargetId); item.SetIcon(input.Icon); item.SetCssClass(input.CssClass); item.SetPermissionName(input.PermissionName); item.SetComponentName(input.ComponentName); item.SetMetadataJson(input.MetadataJson); if (input.IsActive) item.Activate(); else item.Deactivate(); if (input.IsVisible) item.Show(); else item.Hide();
    }

    protected virtual void ApplyInput(MenuItem item, UpdateMenuItemDto input)
    {
        item.SetDescription(input.Description); item.SetKind(input.Kind); item.SetDisplayType(input.DisplayType); item.SetLink(input.Url, input.LinkTarget); item.SetTarget(input.TargetType, input.TargetId); item.SetIcon(input.Icon); item.SetCssClass(input.CssClass); item.SetPermissionName(input.PermissionName); item.SetComponentName(input.ComponentName); item.SetMetadataJson(input.MetadataJson); if (input.IsActive) item.Activate(); else item.Deactivate(); if (input.IsVisible) item.Show(); else item.Hide();
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
        if (string.IsNullOrWhiteSpace(input.ContextType) || string.IsNullOrWhiteSpace(input.MenuName)) throw new BusinessException(MenuManagementErrorCodes.MenuNotFound);
        var menu = await _menuRepository.FindByNameAsync(input.ContextType, input.ContextId, input.MenuName, CurrentTenant.Id, includeDetails: false) ?? throw new BusinessException(MenuManagementErrorCodes.MenuNotFound).WithData("Name", input.MenuName);
        return menu.Id;
    }
}

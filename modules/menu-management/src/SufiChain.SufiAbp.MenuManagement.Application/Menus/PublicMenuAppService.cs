using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

[AllowAnonymous]
public class PublicMenuAppService : ApplicationService, IPublicMenuAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;

    public PublicMenuAppService(IMenuRepository menuRepository, IMenuItemRepository menuItemRepository)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
    }

    public virtual async Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName)
    {
        var menu = await _menuRepository.FindByNameAsync(contextType, contextId, menuName, CurrentTenant.Id, includeDetails: false);
        if (menu == null || !menu.IsActive) return [];
        var items = (await _menuItemRepository.GetTreeItemsAsync(menu.Id, CurrentTenant.Id)).Where(x => x.IsActive && x.IsVisible).ToList();
        return BuildTree(items, null);
    }

    public virtual async Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug)
    {
        var menu = await _menuRepository.FindByNameAsync(contextType, contextId, menuName, CurrentTenant.Id, includeDetails: false);
        if (menu == null || !menu.IsActive) return null;
        var item = await _menuItemRepository.FindBySlugAsync(menu.Id, slug, CurrentTenant.Id, includeDetails: false);
        return item is { IsActive: true, IsVisible: true } ? item.ToDto() : null;
    }

    protected virtual List<MenuItemTreeDto> BuildTree(List<MenuItem> items, Guid? parentId) => items.Where(x => x.ParentId == parentId).OrderBy(x => x.DisplayOrder).ThenBy(x => x.DisplayName).Select(x => { var dto = x.ToTreeDto(); dto.Children = BuildTree(items, x.Id); return dto; }).ToList();
}

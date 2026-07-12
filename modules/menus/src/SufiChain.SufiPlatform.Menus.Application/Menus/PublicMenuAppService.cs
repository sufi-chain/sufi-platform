using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Menus.Features;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Menus.Menus;

[RequiresFeature(SufiMenusFeatures.Enable, SufiMenusFeatures.PublicMenus)]
[AllowAnonymous]
public class PublicMenuAppService : SufiApplicationService, IPublicMenuAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly MenuBusinessLocalizationService _businessLocalization;

    public PublicMenuAppService(
        IMenuRepository menuRepository,
        IMenuItemRepository menuItemRepository,
        MenuBusinessLocalizationService businessLocalization)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _businessLocalization = businessLocalization;
    }

    public virtual async Task<List<MenuItemTreeDto>> GetTreeAsync(string contextType, Guid? contextId, string menuName)
    {
        var menu = await _menuRepository.FindByNameAsync(contextType, contextId, menuName, CurrentTenant.Id, includeDetails: false);
        if (menu == null || !menu.IsActive)
        {
            return [];
        }

        var items = (await _menuItemRepository.GetTreeItemsAsync(menu.Id, CurrentTenant.Id))
            .Where(x => x.IsActive && x.IsVisible)
            .ToList();

        return BuildTree(items, null, menu.ContextType);
    }

    public virtual async Task<MenuItemDto?> FindItemBySlugAsync(string contextType, Guid? contextId, string menuName, string slug)
    {
        var menu = await _menuRepository.FindByNameAsync(contextType, contextId, menuName, CurrentTenant.Id, includeDetails: false);
        if (menu == null || !menu.IsActive)
        {
            return null;
        }

        var item = await _menuItemRepository.FindBySlugAsync(menu.Id, slug, CurrentTenant.Id, includeDetails: false);
        if (item is not { IsActive: true, IsVisible: true })
        {
            return null;
        }

        var dto = item.ToDto();
        dto.DisplayName = _businessLocalization.ResolveMenuItemDisplayName(dto.DisplayName, menu.ContextType);
        return dto;
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
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Menus.Features;
using SufiChain.SufiPlatform.Menus.Permissions;
using Volo.Abp;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Features;

namespace SufiChain.SufiPlatform.Menus.Menus;

[RequiresFeature(SufiMenusFeatures.Enable, SufiMenusFeatures.Menus)]
[Authorize(MenusPermissions.Menus.Default)]
public class MenuAppService : SufiApplicationService, IMenuAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly MenuManager _menuManager;

    public MenuAppService(IMenuRepository menuRepository, IMenuItemRepository menuItemRepository, MenuManager menuManager)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _menuManager = menuManager;
    }

    public virtual async Task<MenuDto> GetAsync(Guid id) => (await _menuRepository.GetAsync(id)).ToDto();

    public virtual async Task<PagedResultDto<MenuListDto>> GetListAsync(GetMenusInput input)
    {
        var menus = await _menuRepository.GetListAsync(includeDetails: false);
        var query = menus.AsEnumerable();
        if (!string.IsNullOrWhiteSpace(input.ContextType)) query = query.Where(x => x.ContextType == input.ContextType);
        if (input.ContextId.HasValue) query = query.Where(x => x.ContextId == input.ContextId.Value);
        if (!string.IsNullOrWhiteSpace(input.Keyword)) query = query.Where(x => x.Name.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase) || x.DisplayName.Contains(input.Keyword, StringComparison.OrdinalIgnoreCase));
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        query = ApplySorting(query, input.Sorting);
        var total = query.LongCount();
        var items = query.Skip(input.SkipCount).Take(input.MaxResultCount).Select(x => x.ToListDto()).ToList();
        return new PagedResultDto<MenuListDto>(total, items);
    }

    public virtual async Task<MenuDto> GetByNameAsync(string contextType, Guid? contextId, string name)
    {
        var menu = await _menuRepository.FindByNameAsync(contextType, contextId, name, CurrentTenant.Id) ?? throw new BusinessException(MenusErrorCodes.MenuNotFound).WithData("Name", name);
        return menu.ToDto();
    }

    [Authorize(MenusPermissions.Menus.Create)]
    public virtual async Task<MenuDto> CreateAsync(CreateMenuDto input)
    {
        var menu = await _menuManager.CreateMenuAsync(input.ContextType, input.ContextId, input.Name, input.DisplayName, CurrentTenant.Id);
        menu.SetDescription(input.Description);
        await _menuRepository.InsertAsync(menu, autoSave: true);
        return menu.ToDto();
    }

    [Authorize(MenusPermissions.Menus.Edit)]
    public virtual async Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input)
    {
        var menu = await _menuRepository.GetAsync(id);
        menu.SetDisplayName(input.DisplayName);
        menu.SetDescription(input.Description);
        if (input.IsActive) menu.Activate(); else menu.Deactivate();
        await _menuRepository.UpdateAsync(menu, autoSave: true);
        return menu.ToDto();
    }

    [Authorize(MenusPermissions.Menus.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var items = await _menuItemRepository.GetTreeItemsAsync(id, CurrentTenant.Id);
        if (items.Count > 0) throw new BusinessException(MenusErrorCodes.CannotDeleteMenuWithItems).WithData("MenuId", id);
        await _menuRepository.DeleteAsync(id, autoSave: true);
    }

    protected virtual IEnumerable<Menu> ApplySorting(IEnumerable<Menu> query, string? sorting) => sorting?.Trim().ToLowerInvariant() switch
    {
        "name" => query.OrderBy(x => x.Name),
        "name desc" => query.OrderByDescending(x => x.Name),
        "displayname desc" => query.OrderByDescending(x => x.DisplayName),
        _ => query.OrderBy(x => x.DisplayName)
    };
}
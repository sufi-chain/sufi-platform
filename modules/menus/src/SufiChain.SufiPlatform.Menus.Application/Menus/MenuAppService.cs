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
public class MenuAppService : SufiApplicationService, IMenuAppService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly MenuManager _menuManager;
    private readonly IDistributedCache<MenuCacheItem> _menuCache;
    private readonly MenuLabelLocalization _labels;

    public MenuAppService(
        IMenuRepository menuRepository,
        IMenuItemRepository menuItemRepository,
        MenuManager menuManager,
        IDistributedCache<MenuCacheItem> menuCache,
        MenuLabelLocalization labels)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _menuManager = menuManager;
        _menuCache = menuCache;
        _labels = labels;
    }

    public virtual async Task<MenuDto> GetAsync(Guid id)
    {
        var menu = await _menuRepository.GetAsync(id);
        return await ToLabeledDtoAsync(menu);
    }

    public virtual Task<List<MenuLabelCultureDto>> GetLabelCulturesAsync() => _labels.GetCulturesAsync();

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
        var cacheKey = MenuCacheItem.CreateCacheKey(contextType, contextId, name);
        var cached = await _menuCache.GetOrAddAsync(cacheKey, async () =>
        {
            var menu = await _menuRepository.FindByNameAsync(contextType, contextId, name, CurrentTenant.Id)
                ?? throw new BusinessException(MenusErrorCodes.MenuNotFound).WithData("Name", name);
            return new MenuCacheItem { Menu = menu };
        });

        return cached.Menu!.ToDto();
    }

    [Authorize(MenusPermissions.Menus.Create)]
    public virtual async Task<MenuDto> CreateAsync(CreateMenuDto input)
    {
        var provisional = input.DisplayNames != null || string.IsNullOrWhiteSpace(input.DisplayName)
            ? input.Name
            : input.DisplayName;
        var menu = await _menuManager.CreateMenuAsync(input.ContextType, input.ContextId, input.Name, provisional, CurrentTenant.Id);
        menu.SetDescription(input.Description);
        if (input.DisplayNames != null)
        {
            await _labels.StoreAsync(
                menu.DisplayName,
                MenuDisplayNamePlanner.MenuKey(menu.Id),
                input.DisplayNames,
                menu.SetDisplayName,
                input.ContextType);
        }

        await _menuRepository.InsertAsync(menu, autoSave: true);
        return await ToLabeledDtoAsync(menu);
    }

    [Authorize(MenusPermissions.Menus.Edit)]
    public virtual async Task<MenuDto> UpdateAsync(Guid id, UpdateMenuDto input)
    {
        var menu = await _menuRepository.GetAsync(id);
        if (input.DisplayNames != null)
        {
            await _labels.StoreAsync(
                menu.DisplayName,
                MenuDisplayNamePlanner.MenuKey(menu.Id),
                input.DisplayNames,
                menu.SetDisplayName,
                menu.ContextType);
        }
        else
        {
            menu.SetDisplayName(DisplayNameOrName(input.DisplayName, menu.Name));
        }

        menu.SetDescription(input.Description);
        if (input.IsActive) menu.Activate(); else menu.Deactivate();
        await _menuRepository.UpdateAsync(menu, autoSave: true);
        return await ToLabeledDtoAsync(menu);
    }

    [Authorize(MenusPermissions.Menus.Delete)]
    public virtual async Task DeleteAsync(Guid id)
    {
        var items = await _menuItemRepository.GetTreeItemsAsync(id, CurrentTenant.Id);
        if (items.Count > 0) throw new BusinessException(MenusErrorCodes.CannotDeleteMenuWithItems).WithData("MenuId", id);
        await _menuRepository.DeleteAsync(id, autoSave: true);
    }

    protected virtual async Task<MenuDto> ToLabeledDtoAsync(Menu menu)
    {
        var dto = menu.ToDto();
        var labels = await _labels.ReadAsync(menu.DisplayName, menu.Name, menu.ContextType);
        dto.DisplayNames = labels.Values;
        dto.DisplayNameBases = labels.BaseValues;
        return dto;
    }

    protected virtual string DisplayNameOrName(string? displayName, string name)
    {
        if (string.IsNullOrWhiteSpace(displayName) || BusinessTextEditorStorage.IsPlaceholder(displayName))
        {
            return name;
        }

        return displayName;
    }

    protected virtual IEnumerable<Menu> ApplySorting(IEnumerable<Menu> query, string? sorting) => sorting?.Trim().ToLowerInvariant() switch
    {
        "name" => query.OrderBy(x => x.Name),
        "name desc" => query.OrderByDescending(x => x.Name),
        "displayname desc" => query.OrderByDescending(x => x.DisplayName),
        _ => query.OrderBy(x => x.DisplayName)
    };
}

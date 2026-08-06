using System.Text.RegularExpressions;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Menus.Features;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.Menus.Menus;

public class MenuManager : DomainService
{
    private readonly IMenuRepository _menuRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IFeatureChecker _featureChecker;

    public MenuManager(
        IMenuRepository menuRepository,
        IMenuItemRepository menuItemRepository,
        IFeatureChecker featureChecker)
    {
        _menuRepository = menuRepository;
        _menuItemRepository = menuItemRepository;
        _featureChecker = featureChecker;
    }

    public virtual async Task<Menu> CreateMenuAsync(string contextType, Guid? contextId, string name, string displayName, Guid? tenantId = null)
    {
        await CheckMenusFeatureAsync();

        await EnsureMenuUniqueAsync(contextType, contextId, name, tenantId);
        return new Menu(GuidGenerator.Create(), contextType, contextId, name, displayName, tenantId);
    }

    public virtual async Task<MenuItem> CreateItemAsync(Guid menuId, string name, string displayName, string? slug, Guid? parentId = null, Guid? tenantId = null)
    {
        await CheckMenusFeatureAsync();

        var normalizedSlug = NormalizeSlug(string.IsNullOrWhiteSpace(slug) ? displayName : slug);
        await EnsureItemSlugUniqueAsync(menuId, normalizedSlug, tenantId);
        var item = new MenuItem(GuidGenerator.Create(), menuId, name, displayName, normalizedSlug, parentId, tenantId);
        await EnsureNoCircularReferenceAsync(item, parentId, tenantId);
        return item;
    }

    public virtual async Task ChangeItemSlugAsync(MenuItem item, string slug)
    {
        await CheckMenusFeatureAsync();

        var normalizedSlug = NormalizeSlug(slug);
        await EnsureItemSlugUniqueAsync(item.MenuId, normalizedSlug, item.TenantId, item.Id);
        item.SetSlug(normalizedSlug);
    }

    public virtual async Task MoveItemAsync(MenuItem item, Guid? parentId, int displayOrder)
    {
        await CheckMenusFeatureAsync();

        await EnsureNoCircularReferenceAsync(item, parentId, item.TenantId);
        item.Move(parentId, displayOrder);
    }

    public virtual Task ValidateItemAsync(MenuItem item)
    {
        if ((item.Kind == MenuItemKind.ExternalUrl || item.Kind == MenuItemKind.InternalRoute) && string.IsNullOrWhiteSpace(item.Url))
        {
            throw new BusinessException(MenusErrorCodes.MenuItemInvalidTarget).WithData("Kind", item.Kind);
        }
        if (item.Kind == MenuItemKind.EntityTarget && (string.IsNullOrWhiteSpace(item.TargetType) || !item.TargetId.HasValue))
        {
            throw new BusinessException(MenusErrorCodes.MenuItemInvalidTarget).WithData("Kind", item.Kind);
        }
        return Task.CompletedTask;
    }

    protected virtual async Task EnsureMenuUniqueAsync(string contextType, Guid? contextId, string name, Guid? tenantId)
    {
        if (await _menuRepository.FindByNameAsync(contextType, contextId, name, tenantId, includeDetails: false) != null)
        {
            throw new BusinessException(MenusErrorCodes.MenuAlreadyExists).WithData("Name", name);
        }
    }

    protected virtual async Task EnsureItemSlugUniqueAsync(Guid menuId, string slug, Guid? tenantId, Guid? currentItemId = null)
    {
        var existing = await _menuItemRepository.FindBySlugAsync(menuId, slug, tenantId, includeDetails: false);
        if (existing != null && existing.Id != currentItemId)
        {
            throw new BusinessException(MenusErrorCodes.MenuItemSlugAlreadyExists).WithData("Slug", slug);
        }
    }

    protected virtual async Task EnsureNoCircularReferenceAsync(MenuItem item, Guid? parentId, Guid? tenantId)
    {
        var currentParentId = parentId;
        while (currentParentId.HasValue)
        {
            if (currentParentId.Value == item.Id)
            {
                throw new BusinessException(MenusErrorCodes.MenuItemCircularReference).WithData("MenuItemId", item.Id);
            }
            var parent = await _menuItemRepository.FindAsync(currentParentId.Value);
            if (parent != null && parent.MenuId != item.MenuId)
            {
                throw new BusinessException(MenusErrorCodes.CannotMoveMenuItemAcrossMenus).WithData("MenuItemId", item.Id);
            }
            currentParentId = parent?.ParentId;
        }
    }

    protected virtual string NormalizeSlug(string value)
    {
        var slug = Regex.Replace(value.Trim().ToLowerInvariant(), @"[^a-z0-9\u0600-\u06FF]+", "-").Trim('-');
        return string.IsNullOrWhiteSpace(slug) ? GuidGenerator.Create().ToString("N") : slug;
    }

    protected virtual async Task CheckMenusFeatureAsync()
    {
        if (!await _featureChecker.IsEnabledAsync(SufiMenusFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SufiMenusFeatures.Enable}");
        }

        if (!await _featureChecker.IsEnabledAsync(SufiMenusFeatures.Menus))
        {
            throw new BusinessException($"Feature is disabled: {SufiMenusFeatures.Menus}");
        }
    }
}
using System.Text.Json;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

public class MenuItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; protected set; }
    public Guid MenuId { get; protected set; }
    public Guid? ParentId { get; protected set; }
    public string Name { get; protected set; } = string.Empty;
    public string DisplayName { get; protected set; } = string.Empty;
    public string Slug { get; protected set; } = string.Empty;
    public string? Description { get; protected set; }
    public int DisplayOrder { get; protected set; }
    public MenuItemKind Kind { get; protected set; }
    public MenuItemDisplayType DisplayType { get; protected set; }
    public string? Url { get; protected set; }
    public MenuLinkTarget LinkTarget { get; protected set; }
    public string? TargetType { get; protected set; }
    public Guid? TargetId { get; protected set; }
    public string? Icon { get; protected set; }
    public string? CssClass { get; protected set; }
    public string? PermissionName { get; protected set; }
    public string? ComponentName { get; protected set; }
    public string? MetadataJson { get; protected set; }
    public bool IsActive { get; protected set; }
    public bool IsVisible { get; protected set; }

    protected MenuItem() { }

    public MenuItem(Guid id, Guid menuId, string name, string displayName, string slug, Guid? parentId = null, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        MenuId = menuId;
        ParentId = parentId;
        SetName(name);
        SetDisplayName(displayName);
        SetSlug(slug);
        Kind = MenuItemKind.Container;
        DisplayType = MenuItemDisplayType.Default;
        LinkTarget = MenuLinkTarget.SameTab;
        IsActive = true;
        IsVisible = true;
    }

    public virtual void SetName(string name) => Name = Check.NotNullOrWhiteSpace(name, nameof(name), MenuManagementConsts.MaxItemNameLength);
    public virtual void SetDisplayName(string displayName) => DisplayName = Check.NotNullOrWhiteSpace(displayName, nameof(displayName), MenuManagementConsts.MaxDisplayNameLength);
    public virtual void SetSlug(string slug) => Slug = Check.NotNullOrWhiteSpace(slug, nameof(slug), MenuManagementConsts.MaxSlugLength);
    public virtual void SetDescription(string? description) => Description = CheckLength(description, MenuManagementConsts.MaxDescriptionLength, nameof(description));
    public virtual void SetParent(Guid? parentId)
    {
        if (parentId == Id)
        {
            throw new BusinessException(MenuManagementErrorCodes.MenuItemCircularReference).WithData("MenuItemId", Id);
        }
        ParentId = parentId;
    }
    public virtual void Move(Guid? parentId, int displayOrder)
    {
        SetParent(parentId);
        Reorder(displayOrder);
    }
    public virtual void Reorder(int displayOrder) => DisplayOrder = displayOrder;
    public virtual void SetKind(MenuItemKind kind) => Kind = kind;
    public virtual void SetDisplayType(MenuItemDisplayType displayType) => DisplayType = displayType;
    public virtual void SetLink(string? url, MenuLinkTarget linkTarget)
    {
        Url = CheckLength(url, MenuManagementConsts.MaxUrlLength, nameof(url));
        LinkTarget = linkTarget;
    }
    public virtual void SetTarget(string? targetType, Guid? targetId)
    {
        TargetType = CheckLength(targetType, MenuManagementConsts.MaxTargetTypeLength, nameof(targetType));
        TargetId = targetId;
    }
    public virtual void SetIcon(string? icon) => Icon = CheckLength(icon, MenuManagementConsts.MaxIconLength, nameof(icon));
    public virtual void SetCssClass(string? cssClass) => CssClass = CheckLength(cssClass, MenuManagementConsts.MaxCssClassLength, nameof(cssClass));
    public virtual void SetPermissionName(string? permissionName) => PermissionName = CheckLength(permissionName, MenuManagementConsts.MaxPermissionNameLength, nameof(permissionName));
    public virtual void SetComponentName(string? componentName) => ComponentName = CheckLength(componentName, MenuManagementConsts.MaxComponentNameLength, nameof(componentName));
    public virtual void SetMetadataJson(string? metadataJson)
    {
        if (!string.IsNullOrWhiteSpace(metadataJson))
        {
            if (metadataJson.Length > MenuManagementConsts.MaxMetadataJsonLength)
            {
                throw new BusinessException(MenuManagementErrorCodes.MenuItemMetadataTooLong).WithData("MaxLength", MenuManagementConsts.MaxMetadataJsonLength);
            }
            JsonDocument.Parse(metadataJson);
        }
        MetadataJson = metadataJson;
    }
    public virtual void Activate() => IsActive = true;
    public virtual void Deactivate() => IsActive = false;
    public virtual void Show() => IsVisible = true;
    public virtual void Hide() => IsVisible = false;

    protected virtual string? CheckLength(string? value, int maxLength, string parameterName)
    {
        return !string.IsNullOrWhiteSpace(value) && value.Length > maxLength ? throw new ArgumentException($"{parameterName} length exceeds {maxLength}.", parameterName) : value;
    }
}

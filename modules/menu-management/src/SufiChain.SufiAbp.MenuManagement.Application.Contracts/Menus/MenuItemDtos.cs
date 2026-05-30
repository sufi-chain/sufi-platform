using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.MenuManagement.Menus;

public class MenuItemDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public MenuItemKind Kind { get; set; }
    public MenuItemDisplayType DisplayType { get; set; }
    public string? Url { get; set; }
    public MenuLinkTarget LinkTarget { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string? Icon { get; set; }
    public string? CssClass { get; set; }
    public string? PermissionName { get; set; }
    public string? ComponentName { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsActive { get; set; }
    public bool IsVisible { get; set; }
}

public class MenuItemTreeDto : MenuItemDto
{
    public List<MenuItemTreeDto> Children { get; set; } = [];
}

public class CreateMenuItemDto
{
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public MenuItemKind Kind { get; set; }
    public MenuItemDisplayType DisplayType { get; set; }
    public string? Url { get; set; }
    public MenuLinkTarget LinkTarget { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string? Icon { get; set; }
    public string? CssClass { get; set; }
    public string? PermissionName { get; set; }
    public string? ComponentName { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVisible { get; set; } = true;
}

public class UpdateMenuItemDto
{
    public Guid? ParentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? Slug { get; set; }
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public MenuItemKind Kind { get; set; }
    public MenuItemDisplayType DisplayType { get; set; }
    public string? Url { get; set; }
    public MenuLinkTarget LinkTarget { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string? Icon { get; set; }
    public string? CssClass { get; set; }
    public string? PermissionName { get; set; }
    public string? ComponentName { get; set; }
    public string? MetadataJson { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVisible { get; set; } = true;
}

public class MoveMenuItemDto
{
    public Guid? ParentId { get; set; }
    public int DisplayOrder { get; set; }
}

public class GetMenuItemsInput : PagedAndSortedResultRequestDto
{
    public Guid MenuId { get; set; }
    public Guid? ParentId { get; set; }
    public string? Keyword { get; set; }
    public MenuItemKind? Kind { get; set; }
    public MenuItemDisplayType? DisplayType { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsVisible { get; set; }
}

public class GetMenuTreeInput
{
    public Guid? MenuId { get; set; }
    public string? ContextType { get; set; }
    public Guid? ContextId { get; set; }
    public string? MenuName { get; set; }
    public bool PublicOnly { get; set; }
}

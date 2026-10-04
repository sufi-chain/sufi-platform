namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Anonymous menu item. It carries the fields a public page renders and omits tenant, permission, and audit identity.
/// </summary>
public class PublicMenuItemDto
{
    public Guid Id { get; set; }
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
    public Dictionary<string, string>? CultureUrls { get; set; }
    public MenuLinkTarget LinkTarget { get; set; }
    public string? TargetType { get; set; }
    public Guid? TargetId { get; set; }
    public string? Icon { get; set; }
    public string? CssClass { get; set; }
    public string? ComponentName { get; set; }
    public bool IsActive { get; set; }
    public bool IsVisible { get; set; }
}

public class PublicMenuItemTreeDto : PublicMenuItemDto
{
    public List<PublicMenuItemTreeDto> Children { get; set; } = [];
}

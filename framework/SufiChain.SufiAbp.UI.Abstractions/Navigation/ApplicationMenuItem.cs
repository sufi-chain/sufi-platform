namespace SufiChain.SufiAbp.UI.Navigation;

/// <summary>
/// Represents a menu item in an application menu.
/// </summary>
public class ApplicationMenuItem : IHasMenuItems
{
    private string _displayName = default!;
    private string? _elementId;

    /// <summary>
    /// Default order value for menu items.
    /// </summary>
    public const int DefaultOrder = 1000;

    /// <summary>
    /// Unique name of the menu item.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Display name of the menu item.
    /// </summary>
    public string DisplayName
    {
        get => _displayName;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("DisplayName cannot be null or whitespace.", nameof(value));
            _displayName = value;
        }
    }

    /// <summary>
    /// Display order of the menu item. Default: 1000.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// The URL to navigate when this menu item is selected.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Icon class or name for the menu item.
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// Returns true if this menu item has no child items.
    /// </summary>
    public bool IsLeaf => Items.Count == 0;

    /// <summary>
    /// Target of the menu item (_blank, _self, etc.).
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// Whether this menu item is disabled.
    /// </summary>
    public bool IsDisabled { get; set; }

    /// <summary>
    /// Child menu items.
    /// </summary>
    public ApplicationMenuItemList Items { get; }

    /// <summary>
    /// Permission name required to see this menu item.
    /// </summary>
    public string? RequiredPermissionName { get; set; }

    /// <summary>
    /// Custom data associated with this menu item.
    /// </summary>
    public Dictionary<string, object> CustomData { get; } = new();

    /// <summary>
    /// DOM element ID for the menu item.
    /// </summary>
    public string? ElementId
    {
        get => _elementId;
        set => _elementId = NormalizeElementId(value);
    }

    /// <summary>
    /// CSS class for styling.
    /// </summary>
    public string? CssClass { get; set; }

    /// <summary>
    /// Group name for grouping menu items.
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// When this item is a main parent (e.g. Sufi Blazor Demo, Administration): if true (default),
    /// its direct children start expanded on load; if false, all children and sub-children start collapsed.
    /// </summary>
    public bool IsCollapsed { get; set; } = true;

    /// <summary>
    /// Optional raw HTML (e.g. SVG markup) to render instead of the Icon when not null.
    /// When set, this content is rendered in place of the SbIcon for this menu item.
    /// </summary>
    public string? CustomContent { get; set; }

    /// <summary>
    /// Creates a new ApplicationMenuItem.
    /// </summary>
    public ApplicationMenuItem(
        string name,
        string displayName,
        string? url = null,
        string? icon = null,
        int order = DefaultOrder,
        string? target = null,
        string? elementId = null,
        string? cssClass = null,
        string? groupName = null,
        string? requiredPermissionName = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or whitespace.", nameof(name));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("DisplayName cannot be null or whitespace.", nameof(displayName));

        Name = name;
        DisplayName = displayName;
        Url = url;
        Icon = icon;
        Order = order;
        Target = target;
        ElementId = elementId ?? GetDefaultElementId();
        CssClass = cssClass;
        GroupName = groupName;
        RequiredPermissionName = requiredPermissionName;
        Items = new ApplicationMenuItemList();
    }

    /// <summary>
    /// Adds a child menu item.
    /// </summary>
    public ApplicationMenuItem AddItem(ApplicationMenuItem menuItem)
    {
        Items.Add(menuItem);
        return this;
    }

    /// <summary>
    /// Adds custom data.
    /// </summary>
    public ApplicationMenuItem WithCustomData(string key, object value)
    {
        CustomData[key] = value;
        return this;
    }

    private string GetDefaultElementId()
    {
        return "MenuItem_" + Name;
    }

    private string? NormalizeElementId(string? elementId)
    {
        return elementId?.Replace(".", "_");
    }

    public override string ToString()
    {
        return $"[ApplicationMenuItem] Name = {Name}";
    }
}

namespace SufiChain.SufiPlatform.UI.Toolbars;

/// <summary>
/// Represents an item in a toolbar.
/// </summary>
public class ToolbarItem
{
    private Type _componentType = default!;

    /// <summary>
    /// The Blazor component type to render for this toolbar item.
    /// </summary>
    public Type ComponentType
    {
        get => _componentType;
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _componentType = value;
        }
    }

    /// <summary>
    /// The order of this item within the toolbar. Lower values appear first.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Creates a new ToolbarItem.
    /// </summary>
    /// <param name="componentType">The Blazor component type to render.</param>
    /// <param name="order">The display order (default: 0).</param>
    /// <exception cref="ArgumentNullException">Thrown when componentType is null.</exception>
    public ToolbarItem(Type componentType, int order = 0)
    {
        if (componentType == null) throw new ArgumentNullException(nameof(componentType));
        _componentType = componentType;
        Order = order;
    }
}

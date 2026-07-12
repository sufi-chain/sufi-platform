namespace SufiChain.SufiPlatform.UI.PageToolbars;

/// <summary>
/// Represents an item in a page toolbar.
/// </summary>
public class PageToolbarItem
{
    /// <summary>
    /// The Blazor component type to render for this toolbar item.
    /// </summary>
    public Type ComponentType { get; }

    /// <summary>
    /// Arguments to pass to the component as parameter name-value pairs.
    /// </summary>
    public Dictionary<string, object?>? Arguments { get; set; }

    /// <summary>
    /// The order of this item within the toolbar. Lower values appear first.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Creates a new PageToolbarItem.
    /// </summary>
    /// <param name="componentType">The Blazor component type to render.</param>
    /// <param name="arguments">Optional arguments for the component.</param>
    /// <param name="order">The display order (default: 0).</param>
    public PageToolbarItem(Type componentType, Dictionary<string, object?>? arguments = null, int order = 0)
    {
        ComponentType = componentType ?? throw new ArgumentNullException(nameof(componentType));
        Arguments = arguments;
        Order = order;
    }
}

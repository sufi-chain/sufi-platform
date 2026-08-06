namespace SufiChain.SufiPlatform.UI.Layout;

/// <summary>
/// Represents an item in a breadcrumb navigation.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// The display text of the breadcrumb item.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Optional icon for the breadcrumb item.
    /// </summary>
    public object? Icon { get; set; }

    /// <summary>
    /// Optional URL to navigate to when clicked.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// Creates a new BreadcrumbItem.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="url">Optional URL.</param>
    /// <param name="icon">Optional icon.</param>
    public BreadcrumbItem(string text, string? url = null, object? icon = null)
    {
        Text = text;
        Url = url;
        Icon = icon;
    }
}

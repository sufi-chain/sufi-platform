namespace SufiChain.SufiPlatform.UI.Layout;

/// <summary>
/// Service for generating breadcrumbs based on the current URL and menu hierarchy.
/// </summary>
public interface IBreadcrumbService
{
    /// <summary>
    /// Gets breadcrumb items for the specified URL by matching against the menu hierarchy.
    /// </summary>
    /// <param name="url">The current URL (full URI).</param>
    /// <returns>A list of breadcrumb items representing the path to the current page.</returns>
    Task<List<BreadcrumbItem>> GetBreadcrumbsForUrlAsync(string url);
}

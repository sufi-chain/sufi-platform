using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.UI.Services.Layout;

/// <summary>
/// Default implementation of IBreadcrumbService that generates breadcrumbs
/// by matching the current URL against the menu hierarchy.
/// </summary>
public class DefaultBreadcrumbService : IBreadcrumbService
{
    private readonly IMenuManager _menuManager;
    
    // Cache menu to avoid repeated loading
    private ApplicationMenu? _cachedMenu;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public DefaultBreadcrumbService(IMenuManager menuManager)
    {
        _menuManager = menuManager;
    }

    /// <inheritdoc/>
    public async Task<List<BreadcrumbItem>> GetBreadcrumbsForUrlAsync(string url)
    {
        var breadcrumbs = new List<BreadcrumbItem>();
        
        // Extract path from full URL
        var path = GetPathFromUrl(url);
        if (string.IsNullOrEmpty(path) || path == "/")
        {
            return breadcrumbs;
        }

        // Get menu (cached)
        var menu = await GetMenuAsync();
        if (menu == null)
        {
            return breadcrumbs;
        }

        // Find the menu item matching the current URL and build breadcrumb trail
        var trail = new List<ApplicationMenuItem>();
        if (FindMenuItemPath(menu.Items, path, trail))
        {
            // Build breadcrumbs from the trail
            foreach (var item in trail)
            {
                var isLast = item == trail[trail.Count - 1];
                breadcrumbs.Add(new BreadcrumbItem(
                    item.DisplayName,
                    isLast ? null : item.Url, // Last item has no link
                    item.Icon
                ));
            }
        }

        return breadcrumbs;
    }

    /// <summary>
    /// Recursively searches the menu tree to find the item matching targetPath
    /// and builds a trail of items from root to target.
    /// </summary>
    private bool FindMenuItemPath(
        IEnumerable<ApplicationMenuItem> items,
        string targetPath,
        List<ApplicationMenuItem> trail)
    {
        foreach (var item in items)
        {
            // Add current item to trail
            trail.Add(item);

            // Check if this item matches
            if (PathMatches(item.Url, targetPath))
            {
                return true;
            }

            // Recursively check children
            if (item.Items != null && item.Items.Any())
            {
                if (FindMenuItemPath(item.Items, targetPath, trail))
                {
                    return true;
                }
            }

            // No match found in this branch, remove from trail
            trail.RemoveAt(trail.Count - 1);
        }

        return false;
    }

    /// <summary>
    /// Checks if a menu path matches the target path.
    /// Handles exact matches and parent path matches.
    /// </summary>
    private bool PathMatches(string? menuPath, string targetPath)
    {
        if (string.IsNullOrEmpty(menuPath))
            return false;

        // Normalize paths (remove trailing slashes)
        menuPath = menuPath.TrimEnd('/');
        targetPath = targetPath.TrimEnd('/');

        // Exact match
        if (string.Equals(menuPath, targetPath, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check if target is a child path of menu path
        // e.g., menu="/admin" should match target="/admin/users"
        if (targetPath.StartsWith(menuPath + "/", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }

    /// <summary>
    /// Extracts the path component from a full URL.
    /// </summary>
    private string GetPathFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
            return string.Empty;

        // If it's already just a path (starts with /), return it
        if (url.StartsWith("/"))
        {
            var path = url;
            
            // Remove query string and fragment
            var questionMarkIndex = path.IndexOf('?');
            if (questionMarkIndex >= 0)
                path = path.Substring(0, questionMarkIndex);
            
            var hashIndex = path.IndexOf('#');
            if (hashIndex >= 0)
                path = path.Substring(0, hashIndex);
            
            return path;
        }

        // Try to parse as full URL
        if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return uri.AbsolutePath;
        }

        // Fallback: return as-is
        return url;
    }

    /// <summary>
    /// Gets the menu, using cache if available.
    /// </summary>
    private async Task<ApplicationMenu?> GetMenuAsync()
    {
        if (_cachedMenu != null)
            return _cachedMenu;

        await _cacheLock.WaitAsync();
        try
        {
            if (_cachedMenu != null)
                return _cachedMenu;

            _cachedMenu = await _menuManager.GetMainMenuAsync();
            return _cachedMenu;
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}

using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.UI.Services.Layout;

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

        var trail = new List<ApplicationMenuItem>();
        var bestScore = -1;
        FindBestMenuPath(menu.Items, path, new List<ApplicationMenuItem>(), ref trail, ref bestScore);
        if (bestScore < 0)
        {
            return breadcrumbs;
        }

        foreach (var item in trail)
        {
            var isLast = item == trail[^1];
            breadcrumbs.Add(new BreadcrumbItem(
                item.DisplayName,
                isLast ? null : item.Url,
                item.Icon
            ));
        }

        return breadcrumbs;
    }

    /// <summary>
    /// Picks the deepest menu item whose URL matches the target.
    /// An exact URL wins over a shorter parent prefix such as
    /// /payments matching /payments/gateways.
    /// </summary>
    private static void FindBestMenuPath(
        IEnumerable<ApplicationMenuItem> items,
        string targetPath,
        List<ApplicationMenuItem> current,
        ref List<ApplicationMenuItem> best,
        ref int bestScore)
    {
        foreach (var item in items)
        {
            current.Add(item);
            var score = MatchScore(item.Url, targetPath);
            if (score > bestScore)
            {
                bestScore = score;
                best = current.ToList();
            }

            if (score != int.MaxValue && item.Items is { Count: > 0 })
            {
                FindBestMenuPath(item.Items, targetPath, current, ref best, ref bestScore);
            }

            current.RemoveAt(current.Count - 1);
        }
    }

    private static int MatchScore(string? menuPath, string targetPath)
    {
        if (string.IsNullOrEmpty(menuPath))
        {
            return -1;
        }

        menuPath = menuPath.TrimEnd('/');
        targetPath = targetPath.TrimEnd('/');
        if (string.Equals(menuPath, targetPath, StringComparison.OrdinalIgnoreCase))
        {
            return int.MaxValue;
        }

        if (targetPath.StartsWith(menuPath + "/", StringComparison.OrdinalIgnoreCase))
        {
            return menuPath.Length;
        }

        return -1;
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

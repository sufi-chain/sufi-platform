using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.Authorization;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Users;
using NavigationApplicationMenu = SufiChain.SufiPlatform.UI.Navigation.ApplicationMenu;
using NavigationApplicationMenuItem = SufiChain.SufiPlatform.UI.Navigation.ApplicationMenuItem;
using NavigationIHasMenuItems = SufiChain.SufiPlatform.UI.Navigation.IHasMenuItems;
using NavigationIMenuContributor = SufiChain.SufiPlatform.UI.Navigation.IMenuContributor;
using NavigationIMenuManager = SufiChain.SufiPlatform.UI.Navigation.IMenuManager;
using NavigationMenuConfigurationContext = SufiChain.SufiPlatform.UI.Navigation.MenuConfigurationContext;

namespace SufiChain.SufiPlatform.UI.Services.Navigation;

/// <summary>
/// Default implementation of IMenuManager.
/// Supports permission-based filtering of menu items.
/// </summary>
public class DefaultMenuManager : NavigationIMenuManager
{
    private readonly System.IServiceProvider _serviceProvider;
    private readonly SufiNavigationOptions _options;
    private readonly ISufiPermissionChecker _permissionChecker;
    private readonly ICurrentUserAccessor _currentUser;

    public DefaultMenuManager(
        System.IServiceProvider serviceProvider,
        IOptions<SufiNavigationOptions> options,
        ISufiPermissionChecker permissionChecker,
        ICurrentUserAccessor currentUser)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _permissionChecker = permissionChecker;
        _currentUser = currentUser;
    }

    /// <inheritdoc/>
    public async Task<NavigationApplicationMenu> GetAsync(string name)
    {
        var menu = new NavigationApplicationMenu(name);
        var context = new NavigationMenuConfigurationContext(menu, _serviceProvider);

        // Get all contributors from options and from DI
        var contributors = _options.MenuContributors
            .Concat(_serviceProvider.GetServices<NavigationIMenuContributor>())
            .ToList();

        foreach (var contributor in contributors)
        {
            await contributor.ConfigureMenuAsync(context);
        }

        // Check permissions and remove unauthorized items
        await CheckPermissionsAsync(menu);

        // Normalize menu items (remove empty, sort by order)
        NormalizeMenu(menu);

        return menu;
    }

    /// <inheritdoc/>
    public Task<NavigationApplicationMenu> GetMainMenuAsync()
    {
        return GetAsync(StandardMenus.Main);
    }

    /// <summary>
    /// Checks permissions for all menu items and removes unauthorized ones.
    /// </summary>
    private async Task CheckPermissionsAsync(NavigationApplicationMenu menu)
    {
        var allMenuItems = new List<NavigationApplicationMenuItem>();
        CollectAllMenuItems(menu, allMenuItems);

        if (allMenuItems.Count == 0)
        {
            return;
        }

        var permissionNames = allMenuItems
            .SelectMany(GetPermissionNames)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        var permissionResults = permissionNames.Count == 0
            ? new Dictionary<string, bool>(StringComparer.Ordinal)
            : await _permissionChecker.IsGrantedAsync(permissionNames);

        var toBeDeleted = new HashSet<NavigationApplicationMenuItem>();
        foreach (var item in allMenuItems)
        {
            if (ShouldRemoveMenuItem(item, permissionResults))
            {
                toBeDeleted.Add(item);
            }
        }

        if (toBeDeleted.Count > 0)
        {
            RemoveMenuItems(menu, toBeDeleted);
        }
    }

    private bool ShouldRemoveMenuItem(
        NavigationApplicationMenuItem item,
        Dictionary<string, bool> permissionResults)
    {
        if (item.IsAuthenticationRequired() && !_currentUser.IsAuthenticated)
        {
            return true;
        }

        if (!string.IsNullOrEmpty(item.RequiredPermissionName))
        {
            if (!permissionResults.TryGetValue(item.RequiredPermissionName, out var isGranted) || !isGranted)
            {
                return true;
            }
        }

        foreach (var permission in item.GetRequiredPermissions())
        {
            if (!permissionResults.TryGetValue(permission, out var isGranted) || !isGranted)
            {
                return true;
            }
        }

        return false;
    }

    private static IEnumerable<string> GetPermissionNames(NavigationApplicationMenuItem item)
    {
        if (!string.IsNullOrEmpty(item.RequiredPermissionName))
        {
            yield return item.RequiredPermissionName;
        }

        foreach (var permission in item.GetRequiredPermissions())
        {
            yield return permission;
        }
    }

    /// <summary>
    /// Recursively collects all menu items from the menu tree.
    /// </summary>
    private void CollectAllMenuItems(NavigationIHasMenuItems menuWithItems, List<NavigationApplicationMenuItem> output)
    {
        foreach (var item in menuWithItems.Items)
        {
            output.Add(item);
            CollectAllMenuItems(item, output);
        }
    }

    /// <summary>
    /// Recursively removes menu items from the tree.
    /// </summary>
    private void RemoveMenuItems(NavigationIHasMenuItems menuWithItems, HashSet<NavigationApplicationMenuItem> toBeDeleted)
    {
        // Remove matching items from this level
        var itemsToRemove = menuWithItems.Items.Where(toBeDeleted.Contains).ToList();
        foreach (var item in itemsToRemove)
        {
            menuWithItems.Items.Remove(item);
        }

        // Recursively process remaining items
        foreach (var item in menuWithItems.Items)
        {
            RemoveMenuItems(item, toBeDeleted);
        }
    }

    private void NormalizeMenu(NavigationApplicationMenu menu)
    {
        menu.Items.Normalize();

        foreach (var item in menu.Items)
        {
            NormalizeMenuItem(item);
        }

        // Remove parent items that have no children after permission filtering
        RemoveEmptyParentItems(menu);
    }

    private void NormalizeMenuItem(NavigationApplicationMenuItem item)
    {
        item.Items.Normalize();

        foreach (var child in item.Items)
        {
            NormalizeMenuItem(child);
        }
    }

    /// <summary>
    /// Removes parent menu items that have no URL and no children.
    /// These become orphaned after child items are removed due to permissions.
    /// </summary>
    private void RemoveEmptyParentItems(NavigationIHasMenuItems menuWithItems)
    {
        // First, recursively process children
        foreach (var item in menuWithItems.Items.ToList())
        {
            RemoveEmptyParentItems(item);
        }

        // Then remove empty parent items (no URL and no children)
        var emptyParents = menuWithItems.Items
            .Where(item => string.IsNullOrEmpty(item.Url) && item.Items.Count == 0)
            .ToList();

        foreach (var item in emptyParents)
        {
            menuWithItems.Items.Remove(item);
        }
    }
}

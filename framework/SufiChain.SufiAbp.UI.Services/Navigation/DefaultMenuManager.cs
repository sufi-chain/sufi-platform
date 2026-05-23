using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.UI.Authorization;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.UI.Services.Navigation;

/// <summary>
/// Default implementation of IMenuManager.
/// Supports permission-based filtering of menu items.
/// </summary>
public class DefaultMenuManager : IMenuManager
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SufiAbpNavigationOptions _options;
    private readonly ISufiAbpPermissionChecker _permissionChecker;

    public DefaultMenuManager(
        IServiceProvider serviceProvider,
        IOptions<SufiAbpNavigationOptions> options,
        ISufiAbpPermissionChecker permissionChecker)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
        _permissionChecker = permissionChecker;
    }

    /// <inheritdoc/>
    public async Task<ApplicationMenu> GetAsync(string name)
    {
        var menu = new ApplicationMenu(name);
        var context = new MenuConfigurationContext(menu, _serviceProvider);

        // Get all contributors from options and from DI
        var contributors = _options.MenuContributors
            .Concat(_serviceProvider.GetServices<IMenuContributor>())
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
    public Task<ApplicationMenu> GetMainMenuAsync()
    {
        return GetAsync(StandardMenus.Main);
    }

    /// <summary>
    /// Checks permissions for all menu items and removes unauthorized ones.
    /// </summary>
    private async Task CheckPermissionsAsync(ApplicationMenu menu)
    {
        // Collect all menu items that require permissions
        var allMenuItems = new List<ApplicationMenuItem>();
        CollectAllMenuItems(menu, allMenuItems);

        // Get unique permission names
        var permissionNames = allMenuItems
            .Where(item => !string.IsNullOrEmpty(item.RequiredPermissionName))
            .Select(item => item.RequiredPermissionName!)
            .Distinct()
            .ToList();

        if (permissionNames.Count == 0)
        {
            return;
        }

        // Batch check permissions for performance
        var permissionResults = await _permissionChecker.IsGrantedAsync(permissionNames);

        // Find items to remove (permission denied)
        var toBeDeleted = new HashSet<ApplicationMenuItem>();
        foreach (var item in allMenuItems)
        {
            if (!string.IsNullOrEmpty(item.RequiredPermissionName) &&
                permissionResults.TryGetValue(item.RequiredPermissionName, out var isGranted) &&
                !isGranted)
            {
                toBeDeleted.Add(item);
            }
        }

        // Remove unauthorized menu items
        if (toBeDeleted.Count > 0)
        {
            RemoveMenuItems(menu, toBeDeleted);
        }
    }

    /// <summary>
    /// Recursively collects all menu items from the menu tree.
    /// </summary>
    private void CollectAllMenuItems(IHasMenuItems menuWithItems, List<ApplicationMenuItem> output)
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
    private void RemoveMenuItems(IHasMenuItems menuWithItems, HashSet<ApplicationMenuItem> toBeDeleted)
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

    private void NormalizeMenu(ApplicationMenu menu)
    {
        menu.Items.Normalize();

        foreach (var item in menu.Items)
        {
            NormalizeMenuItem(item);
        }

        // Remove parent items that have no children after permission filtering
        RemoveEmptyParentItems(menu);
    }

    private void NormalizeMenuItem(ApplicationMenuItem item)
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
    private void RemoveEmptyParentItems(IHasMenuItems menuWithItems)
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

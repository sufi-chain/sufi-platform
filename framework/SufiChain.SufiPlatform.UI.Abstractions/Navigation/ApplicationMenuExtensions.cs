namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Extension methods for ApplicationMenu.
/// </summary>
public static class ApplicationMenuExtensions
{
    /// <summary>
    /// Standard name for the Administration menu group.
    /// </summary>
    public const string AdministrationMenuName = "Administration";

    /// <summary>
    /// Standard name for the Demo menu group.
    /// </summary>
    public const string DemoMenuName = "Demo";

    /// <summary>
    /// Gets or creates the Administration menu item.
    /// </summary>
    public static ApplicationMenuItem GetAdministration(this ApplicationMenu menu)
    {
        var administration = menu.Items.FirstOrDefault(m => m.Name == AdministrationMenuName);
        if (administration == null)
        {
            administration = new ApplicationMenuItem(
                name: AdministrationMenuName,
                displayName: "Administration",
                icon: "settings",
                order: int.MaxValue - 1000 // Put near the end
            )
            {
                IsCollapsed = false // Children start collapsed
            };
            menu.Items.Add(administration);
        }
        return administration;
    }

    /// <summary>
    /// Gets or creates the Demo menu item.
    /// </summary>
    public static ApplicationMenuItem GetDemo(this ApplicationMenu menu)
    {
        var demo = menu.Items.FirstOrDefault(m => m.Name == DemoMenuName);
        if (demo == null)
        {
            demo = new ApplicationMenuItem(
                name: DemoMenuName,
                displayName: "Demo",
                url: "/demo",
                icon: "demo",
                order: 100
            )
            {
                IsCollapsed = false
            };
            menu.Items.Add(demo);
        }
        return demo;
    }

    /// <summary>
    /// Sets the order of a sub-item by name. If the item exists, its Order is updated.
    /// </summary>
    public static IHasMenuItems SetSubItemOrder(this IHasMenuItems menuWithItems, string menuItemName, int order)
    {
        var menuItem = menuWithItems.Items.FirstOrDefault(m => m.Name == menuItemName);
        if (menuItem != null)
        {
            menuItem.Order = order;
        }
        return menuWithItems;
    }

    /// <summary>
    /// Gets a menu item by name, or null if not found.
    /// </summary>
    public static ApplicationMenuItem? GetMenuItem(this ApplicationMenu menu, string name)
    {
        return FindMenuItemRecursive(menu.Items, name);
    }

    private static ApplicationMenuItem? FindMenuItemRecursive(IEnumerable<ApplicationMenuItem> items, string name)
    {
        foreach (var item in items)
        {
            if (item.Name == name)
                return item;

            var found = FindMenuItemRecursive(item.Items, name);
            if (found != null)
                return found;
        }
        return null;
    }
}

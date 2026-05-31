namespace SufiChain.SufiAbp.MenuManagement.Features;

/// <summary>
/// Shared feature names for SufiAbp Menu Management capabilities.
/// </summary>
public static class SufiAbpMenuManagementFeatures
{
    public const string GroupName = "MenuManagement";

    /// <summary>
    /// Master switch for the Menu Management module.
    /// </summary>
    public const string Enable = GroupName + ".Enable";

    /// <summary>
    /// Admin menu and menu item management.
    /// </summary>
    public const string Menus = GroupName + ".Menus";

    /// <summary>
    /// Public menu resolution APIs for front-end sites.
    /// </summary>
    public const string PublicMenus = GroupName + ".PublicMenus";
}

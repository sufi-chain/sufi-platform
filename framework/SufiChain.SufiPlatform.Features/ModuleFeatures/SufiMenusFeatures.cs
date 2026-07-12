namespace SufiChain.SufiPlatform.Menus.Features;

/// <summary>
/// Shared feature names for Sufi Menu Management capabilities.
/// </summary>
public static class SufiMenusFeatures
{
    public const string GroupName = "SufiMenus";

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
namespace SufiChain.SufiPlatform.UI.Navigation;

/// <summary>
/// Extension methods for ApplicationMenuItem.
/// </summary>
public static class ApplicationMenuItemExtensions
{
    /// <summary>
    /// Key for storing required permissions in CustomData.
    /// </summary>
    public const string RequiredPermissionsKey = "RequiredPermissions";
    
    /// <summary>
    /// Key for storing authentication requirement in CustomData.
    /// </summary>
    public const string RequireAuthenticatedKey = "RequireAuthenticated";

    /// <summary>
    /// Marks this menu item as requiring specific permissions.
    /// The menu item will only be shown if the user has all specified permissions.
    /// </summary>
    public static ApplicationMenuItem RequirePermissions(this ApplicationMenuItem item, params string[] permissions)
    {
        if (permissions.Length > 0)
        {
            item.CustomData[RequiredPermissionsKey] = permissions;
        }
        return item;
    }

    /// <summary>
    /// Gets the required permissions for this menu item.
    /// </summary>
    public static string[] GetRequiredPermissions(this ApplicationMenuItem item)
    {
        if (item.CustomData.TryGetValue(RequiredPermissionsKey, out var value) && value is string[] permissions)
        {
            return permissions;
        }
        return Array.Empty<string>();
    }

    /// <summary>
    /// Checks if this menu item has any required permissions.
    /// </summary>
    public static bool HasRequiredPermissions(this ApplicationMenuItem item)
    {
        return item.CustomData.ContainsKey(RequiredPermissionsKey);
    }
    
    /// <summary>
    /// Marks this menu item as requiring authentication.
    /// The menu item will only be shown if the user is authenticated.
    /// </summary>
    public static ApplicationMenuItem RequireAuthenticated(this ApplicationMenuItem item)
    {
        item.CustomData[RequireAuthenticatedKey] = true;
        return item;
    }
    
    /// <summary>
    /// Checks if this menu item requires authentication.
    /// </summary>
    public static bool IsAuthenticationRequired(this ApplicationMenuItem item)
    {
        if (item.CustomData.TryGetValue(RequireAuthenticatedKey, out var value) && value is bool required)
        {
            return required;
        }
        return false;
    }
}

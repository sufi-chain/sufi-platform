namespace SufiChain.SufiPlatform.UI.Authorization;

/// <summary>
/// Abstraction for checking user permissions.
/// Implementations can use various authorization systems (ABP, ASP.NET Core policies, etc.)
/// </summary>
public interface ISufiPermissionChecker
{
    /// <summary>
    /// Checks if the current user has the specified permission.
    /// </summary>
    /// <param name="permissionName">The name of the permission to check.</param>
    /// <returns>True if permission is granted, false otherwise.</returns>
    Task<bool> IsGrantedAsync(string permissionName);

    /// <summary>
    /// Checks multiple permissions at once for better performance.
    /// </summary>
    /// <param name="permissionNames">The permission names to check.</param>
    /// <returns>Dictionary of permission name to grant status.</returns>
    Task<Dictionary<string, bool>> IsGrantedAsync(IEnumerable<string> permissionNames);
}

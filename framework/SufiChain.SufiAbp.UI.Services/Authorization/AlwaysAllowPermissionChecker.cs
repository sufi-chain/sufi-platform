using SufiChain.SufiAbp.UI.Authorization;

namespace SufiChain.SufiAbp.UI.Services.Authorization;

/// <summary>
/// Default permission checker that always grants all permissions.
/// Useful for development or apps without permission requirements.
/// Replace with a product-specific permission checker when authorization is active.
/// </summary>
public class AlwaysAllowPermissionChecker : ISufiAbpPermissionChecker
{
    public Task<bool> IsGrantedAsync(string permissionName)
    {
        return Task.FromResult(true);
    }

    public Task<Dictionary<string, bool>> IsGrantedAsync(IEnumerable<string> permissionNames)
    {
        var result = permissionNames.ToDictionary(p => p, _ => true);
        return Task.FromResult(result);
    }
}

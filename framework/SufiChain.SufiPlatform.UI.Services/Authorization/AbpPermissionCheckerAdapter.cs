using SufiChain.SufiPlatform.UI.Authorization;
using Volo.Abp.Authorization.Permissions;

namespace SufiChain.SufiPlatform.UI.Services.Authorization;

/// <summary>
/// Permission checker that delegates to ABP <see cref="IPermissionChecker"/>.
/// Use in authenticated hosts instead of <see cref="AlwaysAllowPermissionChecker"/>.
/// </summary>
public class AbpPermissionCheckerAdapter : ISufiPermissionChecker
{
    private readonly IPermissionChecker _permissionChecker;

    public AbpPermissionCheckerAdapter(IPermissionChecker permissionChecker)
    {
        _permissionChecker = permissionChecker;
    }

    public Task<bool> IsGrantedAsync(string permissionName)
    {
        return _permissionChecker.IsGrantedAsync(permissionName);
    }

    public async Task<Dictionary<string, bool>> IsGrantedAsync(IEnumerable<string> permissionNames)
    {
        var result = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var permissionName in permissionNames.Distinct(StringComparer.Ordinal))
        {
            result[permissionName] = await _permissionChecker.IsGrantedAsync(permissionName);
        }

        return result;
    }
}

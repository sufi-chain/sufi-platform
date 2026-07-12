using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SufiChain.SufiPlatform.Authorization.Permissions;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Permissions;

public static class RolePermissionManagerExtensions
{
    public static Task<List<PermissionWithGrantedProviders>> GetAllForRoleAsync([NotNull] this IPermissionManager permissionManager, [NotNull] string roleName)
    {
        Check.NotNull(permissionManager, nameof(permissionManager));
        Check.NotNull(roleName, nameof(roleName));

        return permissionManager.GetAllAsync(RolePermissionValueProvider.ProviderName, roleName);
    }

    public static Task SetForRoleAsync([NotNull] this IPermissionManager permissionManager, [NotNull] string roleName, [NotNull] string name, bool isGranted)
    {
        Check.NotNull(permissionManager, nameof(permissionManager));
        Check.NotNull(roleName, nameof(roleName));
        Check.NotNull(name, nameof(name));

        return permissionManager.SetAsync(name, RolePermissionValueProvider.ProviderName, roleName, isGranted);
    }
}

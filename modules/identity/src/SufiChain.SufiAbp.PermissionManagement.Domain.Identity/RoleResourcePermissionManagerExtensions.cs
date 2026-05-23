using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SufiChain.SufiAbp.Authorization.Permissions;
using Volo.Abp;

namespace SufiChain.SufiAbp.PermissionManagement;

public static class RoleResourcePermissionManagerExtensions
{
    public static Task<List<PermissionWithGrantedProviders>> GetAllForRoleAsync(
        [NotNull] this IResourcePermissionManager resourcePermissionManager, 
        [NotNull] string roleName,
        [NotNull] string resourceName,
        [NotNull] string resourceKey)
    {
        Check.NotNull(resourcePermissionManager, nameof(resourcePermissionManager));
        Check.NotNull(roleName, nameof(roleName));
        Check.NotNull(resourceName, nameof(resourceName));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionManager.GetAllAsync(resourceName, resourceKey, RolePermissionValueProvider.ProviderName, roleName);
    }

    public static Task SetForRoleAsync(
        [NotNull] this IResourcePermissionManager resourcePermissionManager, 
        [NotNull] string roleName, 
        [NotNull] string name, 
        [NotNull] string resourceName,
        [NotNull] string resourceKey,
        bool isGranted)
    {
        Check.NotNull(resourcePermissionManager, nameof(resourcePermissionManager));
        Check.NotNull(roleName, nameof(roleName));
        Check.NotNull(name, nameof(name));
        Check.NotNull(resourceName, nameof(resourceName));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionManager.SetAsync(name, resourceName, resourceKey, RolePermissionValueProvider.ProviderName, roleName, isGranted);
    }
}

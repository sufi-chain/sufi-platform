using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using SufiChain.SufiAbp.Authorization.Permissions;
using Volo.Abp;

namespace SufiChain.SufiAbp.PermissionManagement;

public static class UserResourcePermissionManagerExtensions
{
    public static Task<List<PermissionWithGrantedProviders>> GetAllForUserAsync(
        [NotNull] this IResourcePermissionManager resourcePermissionManager, 
        Guid userId,
        [NotNull] string resourceName,
        [NotNull] string resourceKey)
    {
        Check.NotNull(resourcePermissionManager, nameof(resourcePermissionManager));
        Check.NotNull(resourceName, nameof(resourceName));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionManager.GetAllAsync(resourceName, resourceKey, UserPermissionValueProvider.ProviderName, userId.ToString());
    }

    public static Task SetForUserAsync(
        [NotNull] this IResourcePermissionManager resourcePermissionManager, 
        Guid userId, 
        [NotNull] string name, 
        [NotNull] string resourceName,
        [NotNull] string resourceKey,
        bool isGranted)
    {
        Check.NotNull(resourcePermissionManager, nameof(resourcePermissionManager));
        Check.NotNull(name, nameof(name));
        Check.NotNull(resourceName, nameof(resourceName));
        Check.NotNull(resourceKey, nameof(resourceKey));

        return resourcePermissionManager.SetAsync(name, resourceName, resourceKey, UserPermissionValueProvider.ProviderName, userId.ToString(), isGranted);
    }
}

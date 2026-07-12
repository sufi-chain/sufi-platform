using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Guids;
using SufiChain.SufiPlatform.Authorization.Permissions;

namespace SufiChain.SufiPlatform.Permissions.Identity;

public class RoleResourcePermissionsProvider : ResourcePermissionsProvider
{
    public override string Name => RolePermissionValueProvider.ProviderName;

    protected IUserRoleFinder UserRoleFinder { get; }

    public RoleResourcePermissionsProvider(
        IResourcePermissionGrantRepository resourcePermissionGrantRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant,
        IUserRoleFinder userRoleFinder)
        : base(
            resourcePermissionGrantRepository,
            guidGenerator,
            currentTenant)
    {
        UserRoleFinder = userRoleFinder;
    }

    public async override Task<ResourcePermissionValueProviderGrantInfo> CheckAsync(string name, string providerName, string providerKey, string? resourceType, string? resourceId)
    {
        var multipleGrantInfo = await CheckAsync(new[] { name }, providerName, providerKey, resourceType, resourceId);

        return multipleGrantInfo.Result.Values.First();
    }

    public async override Task<MultipleResourcePermissionValueProviderGrantInfo> CheckAsync(string[] names, string providerName, string providerKey, string? resourceType, string? resourceId)
    {
        using (ResourcePermissionGrantRepository.DisableTracking())
        {
            var multiplePermissionValueProviderGrantInfo = new MultipleResourcePermissionValueProviderGrantInfo(names);
            var permissionGrants = new List<ResourcePermissionGrant>();

            if (providerName == Name)
            {
                permissionGrants.AddRange(await ResourcePermissionGrantRepository.GetListAsync(names, providerName, providerKey, resourceType, resourceId));
            }

            if (providerName == UserPermissionValueProvider.ProviderName && Guid.TryParse(providerKey, out var userId))
            {
                var roleNames = await UserRoleFinder.GetRoleNamesAsync(userId);

                foreach (var roleName in roleNames)
                {
                    permissionGrants.AddRange(await ResourcePermissionGrantRepository.GetListAsync(names, Name, roleName, resourceType, resourceId));
                }
            }

            permissionGrants = permissionGrants.Distinct().ToList();
            if (!permissionGrants.Any())
            {
                return multiplePermissionValueProviderGrantInfo;
            }

            foreach (var permissionName in names)
            {
                var permissionGrant = permissionGrants.FirstOrDefault(x => x.Name == permissionName);
                if (permissionGrant != null)
                {
                    multiplePermissionValueProviderGrantInfo.Result[permissionName] = new ResourcePermissionValueProviderGrantInfo(true, permissionGrant.ProviderKey);
                }
            }

            return multiplePermissionValueProviderGrantInfo;
        }
    }
}

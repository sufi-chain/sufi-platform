using SufiChain.SufiPlatform.Authorization.Permissions;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Permissions.Identity;

public class UserResourcePermissionsProvider : ResourcePermissionsProvider
{
    public override string Name => UserPermissionValueProvider.ProviderName;

    public UserResourcePermissionsProvider(
        IResourcePermissionGrantRepository resourcePermissionGrantRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
        : base(
            resourcePermissionGrantRepository,
            guidGenerator,
            currentTenant)
    {

    }
}

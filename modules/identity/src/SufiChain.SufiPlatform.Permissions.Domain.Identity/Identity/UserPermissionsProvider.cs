using SufiChain.SufiPlatform.Authorization.Permissions;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Permissions.Identity;

public class UserPermissionsProvider : PermissionsProvider
{
    public override string Name => UserPermissionValueProvider.ProviderName;

    public UserPermissionsProvider(
        IPermissionGrantRepository permissionGrantRepository,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
        : base(
            permissionGrantRepository,
            guidGenerator,
            currentTenant)
    {

    }
}

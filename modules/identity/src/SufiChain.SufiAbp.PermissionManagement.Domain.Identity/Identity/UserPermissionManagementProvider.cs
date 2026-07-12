using SufiChain.SufiAbp.Authorization.Permissions;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

public class UserPermissionManagementProvider : PermissionManagementProvider
{
    public override string Name => UserPermissionValueProvider.ProviderName;

    public UserPermissionManagementProvider(
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

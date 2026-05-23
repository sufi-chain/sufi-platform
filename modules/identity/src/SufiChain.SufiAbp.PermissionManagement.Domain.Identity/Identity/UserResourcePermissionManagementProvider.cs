using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Guids;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

public class UserResourcePermissionManagementProvider : ResourcePermissionManagementProvider
{
    public override string Name => UserPermissionValueProvider.ProviderName;

    public UserResourcePermissionManagementProvider(
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

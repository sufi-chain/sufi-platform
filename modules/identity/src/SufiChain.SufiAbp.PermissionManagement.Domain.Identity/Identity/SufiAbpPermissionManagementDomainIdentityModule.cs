using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Users;

namespace SufiChain.SufiAbp.PermissionManagement.Identity;

[DependsOn(
    typeof(SufiAbpIdentityApplicationContractsModule),
    typeof(SufiAbpIdentityDomainSharedModule),
    typeof(SufiAbpPermissionManagementDomainModule),
    typeof(SufiAbpUsersAbstractionModule)
)]
public class SufiAbpPermissionManagementDomainIdentityModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<PermissionManagementOptions>(options =>
        {
            options.ManagementProviders.Add<UserPermissionManagementProvider>();
            options.ManagementProviders.Add<RolePermissionManagementProvider>();

            //TODO: Can we prevent duplication of permission names without breaking the design and making the system complicated
            options.ProviderPolicies[UserPermissionValueProvider.ProviderName] = IdentityPermissions.Users.ManagePermissions;
            options.ProviderPolicies[RolePermissionValueProvider.ProviderName] = IdentityPermissions.Roles.ManagePermissions;

            options.ResourceManagementProviders.Add<UserResourcePermissionManagementProvider>();
            options.ResourceManagementProviders.Add<RoleResourcePermissionManagementProvider>();

            options.ResourcePermissionProviderKeyLookupServices.Add<UserResourcePermissionProviderKeyLookupService>();
            options.ResourcePermissionProviderKeyLookupServices.Add<RoleResourcePermissionProviderKeyLookupService>();
        });
    }
}

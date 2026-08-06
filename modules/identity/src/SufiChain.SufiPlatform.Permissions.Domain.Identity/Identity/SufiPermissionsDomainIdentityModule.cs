using SufiChain.SufiPlatform.Authorization.Permissions;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Users;

namespace SufiChain.SufiPlatform.Permissions.Identity;

[DependsOn(
    typeof(SufiIdentityApplicationContractsModule),
    typeof(SufiIdentityDomainSharedModule),
    typeof(SufiPermissionsDomainModule),
    typeof(SufiUsersAbstractionModule)
)]
public class SufiPermissionsDomainIdentityModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<PermissionsOptions>(options =>
        {
            options.ManagementProviders.Add<UserPermissionsProvider>();
            options.ManagementProviders.Add<RolePermissionsProvider>();

            //TODO: Can we prevent duplication of permission names without breaking the design and making the system complicated
            options.ProviderPolicies[UserPermissionValueProvider.ProviderName] = IdentityPermissions.Users.ManagePermissions;
            options.ProviderPolicies[RolePermissionValueProvider.ProviderName] = IdentityPermissions.Roles.ManagePermissions;

            options.ResourceManagementProviders.Add<UserResourcePermissionsProvider>();
            options.ResourceManagementProviders.Add<RoleResourcePermissionsProvider>();

            options.ResourcePermissionProviderKeyLookupServices.Add<UserResourcePermissionProviderKeyLookupService>();
            options.ResourcePermissionProviderKeyLookupServices.Add<RoleResourcePermissionProviderKeyLookupService>();
        });
    }
}

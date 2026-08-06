using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Identity;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule),
    typeof(SufiIdentityDomainSharedModule)
)]
public class SufiIdentityApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.DeletedPermissionGroups.Add("SufiIdentity");
        });
    }
}

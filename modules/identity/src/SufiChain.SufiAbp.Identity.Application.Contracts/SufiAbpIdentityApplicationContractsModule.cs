using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Identity;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule),
    typeof(SufiAbpIdentityDomainSharedModule)
)]
public class SufiAbpIdentityApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.DeletedPermissionGroups.Add("SufiAbpIdentity");
        });
    }
}

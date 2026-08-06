using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Tenants;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiTenantsDomainSharedModule)
)]
public class SufiTenantsApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.DeletedPermissionGroups.Add("SufiTenants");
        });
    }
}

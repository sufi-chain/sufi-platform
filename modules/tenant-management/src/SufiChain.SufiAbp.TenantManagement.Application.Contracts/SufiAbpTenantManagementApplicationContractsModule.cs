using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.TenantManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpTenantManagementDomainSharedModule)
)]
public class SufiAbpTenantManagementApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.DeletedPermissionGroups.Add("SufiAbpTenantManagement");
        });
    }
}

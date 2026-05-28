using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FeatureManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpFeatureManagementDomainSharedModule)
)]
public class SufiAbpFeatureManagementApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.DeletedPermissionGroups.Add("SufiAbpFeatureManagement");
        });
    }
}

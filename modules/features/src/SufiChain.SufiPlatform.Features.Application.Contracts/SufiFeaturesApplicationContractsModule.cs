using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Features;

[DependsOn(
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiFeaturesDomainSharedModule)
)]
public class SufiFeaturesApplicationContractsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpPermissionOptions>(options =>
        {
            options.DeletedPermissionGroups.Add("SufiFeatures");
        });
    }
}

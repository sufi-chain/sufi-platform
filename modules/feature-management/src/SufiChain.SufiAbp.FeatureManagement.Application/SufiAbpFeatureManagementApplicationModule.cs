using SufiChain.SufiAbp.FeatureManagement;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FeatureManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpFeatureManagementDomainModule),
    typeof(SufiAbpFeatureManagementApplicationContractsModule)
)]
public class SufiAbpFeatureManagementApplicationModule : AbpModule
{
}

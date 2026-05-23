using Volo.Abp.Application;
using SufiChain.SufiAbp.FeatureManagement;
using Volo.Abp.Modularity;
using SufiChain.SufiAbp.Ddd;

namespace SufiChain.SufiAbp.FeatureManagement;

[DependsOn(
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpFeatureManagementDomainSharedModule)
)]
public class SufiAbpFeatureManagementApplicationContractsModule : AbpModule
{
}

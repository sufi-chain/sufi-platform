using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Features;

[DependsOn(
    typeof(SufiDddApplicationModule),
    typeof(SufiFeaturesDomainModule),
    typeof(SufiFeaturesApplicationContractsModule)
)]
public class SufiFeaturesApplicationModule : AbpModule
{
}

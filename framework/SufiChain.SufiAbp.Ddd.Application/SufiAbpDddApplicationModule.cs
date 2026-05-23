using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Ddd;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpDddApplicationContractsModule)
)]
public class SufiAbpDddApplicationModule : AbpModule
{
}

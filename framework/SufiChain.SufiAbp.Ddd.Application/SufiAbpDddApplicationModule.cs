using Volo.Abp.Modularity;
using Volo.Abp.Domain;

namespace SufiChain.SufiAbp.Ddd;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiAbpDddApplicationContractsModule)
)]
public class SufiAbpDddApplicationModule : AbpModule
{
}

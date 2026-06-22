using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(AITestBaseModule),
    typeof(SufiAIDomainModule)
)]
public class AIDomainTestModule : AbpModule
{
}

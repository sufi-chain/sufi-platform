using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAITestBaseModule),
    typeof(SufiAIDomainModule)
)]
public class SufiAIDomainTestModule : AbpModule
{
}

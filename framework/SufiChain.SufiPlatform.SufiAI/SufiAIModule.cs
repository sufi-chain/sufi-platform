using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIAbstractionsModule)
)]
public class SufiAIModule : AbpModule
{
}

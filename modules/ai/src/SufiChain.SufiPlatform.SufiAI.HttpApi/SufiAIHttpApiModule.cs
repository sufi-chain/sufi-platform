using SufiChain.SufiPlatform.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAspNetCoreMvcModule)
)]
public class SufiAIHttpApiModule : AbpModule
{
}

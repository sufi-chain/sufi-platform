using SufiChain.SufiAbp.AspNetCore.Mvc;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiAIApplicationModule),
    typeof(SufiAbpAspNetCoreMvcModule)
)]
public class SufiAIHttpApiModule : AbpModule
{
}

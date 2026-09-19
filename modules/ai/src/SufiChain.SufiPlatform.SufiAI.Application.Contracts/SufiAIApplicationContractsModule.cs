using SufiChain.SufiPlatform.Tags;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIDomainSharedModule),
    typeof(SufiTagsAbstractionsModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
)]
public class SufiAIApplicationContractsModule : AbpModule
{
}

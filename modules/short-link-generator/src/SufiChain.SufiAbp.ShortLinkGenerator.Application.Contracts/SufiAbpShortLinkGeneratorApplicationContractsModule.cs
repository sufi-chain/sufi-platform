using SufiChain.SufiAbp.Authorization;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorDomainSharedModule),
    typeof(SufiAbpDddApplicationContractsModule),
    typeof(SufiAbpAuthorizationModule)
    )]
public class SufiAbpShortLinkGeneratorApplicationContractsModule : AbpModule
{

}



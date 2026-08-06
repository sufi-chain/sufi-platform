using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(SufiShortLinksDomainSharedModule),
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule)
    )]
public class SufiShortLinksApplicationContractsModule : AbpModule
{

}
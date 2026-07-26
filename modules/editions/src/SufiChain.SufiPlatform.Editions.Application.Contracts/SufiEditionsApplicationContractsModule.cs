using Volo.Abp.Application;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Editions;

[DependsOn(
    typeof(SufiEditionsDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
)]
public class SufiEditionsApplicationContractsModule : AbpModule
{
}

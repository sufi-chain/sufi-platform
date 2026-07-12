using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;
using SufiChain.SufiPlatform.Authorization;
using SufiChain.SufiPlatform.Ddd;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(SufiLocalizationDomainSharedModule),
    typeof(SufiDddApplicationContractsModule),
    typeof(SufiAuthorizationModule)
)]
public class SufiLocalizationApplicationContractsModule : AbpModule
{
}

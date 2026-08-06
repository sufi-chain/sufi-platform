using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Localization;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(SufiLocalizationDomainSharedModule)
)]
public class SufiLocalizationDomainModule : AbpModule
{
}

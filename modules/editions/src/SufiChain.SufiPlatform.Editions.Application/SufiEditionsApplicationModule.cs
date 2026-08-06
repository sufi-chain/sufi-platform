using Volo.Abp.Mapperly;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Editions;

[DependsOn(
    typeof(SufiEditionsDomainModule),
    typeof(SufiEditionsApplicationContractsModule),
    typeof(AbpMapperlyModule)
)]
public class SufiEditionsApplicationModule : AbpModule
{
}

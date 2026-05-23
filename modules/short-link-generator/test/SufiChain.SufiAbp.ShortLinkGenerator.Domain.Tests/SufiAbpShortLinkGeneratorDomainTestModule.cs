using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(ShortLinkGeneratorDomainModule),
    typeof(ShortLinkGeneratorTestBaseModule)
)]
public class SufiAbpShortLinkGeneratorDomainTestModule : AbpModule
{

}

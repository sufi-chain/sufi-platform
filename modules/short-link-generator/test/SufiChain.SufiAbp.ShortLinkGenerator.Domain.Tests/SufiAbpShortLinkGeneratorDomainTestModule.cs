using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorDomainModule),
    typeof(SufiAbpShortLinkGeneratorTestBaseModule)
)]
public class SufiAbpShortLinkGeneratorDomainTestModule : AbpModule
{

}

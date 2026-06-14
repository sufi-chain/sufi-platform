using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorApplicationModule),
    typeof(SufiAbpShortLinkGeneratorDomainTestModule)
)]
public class SufiAbpShortLinkGeneratorApplicationTestModule : AbpModule
{

}

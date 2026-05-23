using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(ShortLinkGeneratorApplicationModule),
    typeof(ShortLinkGeneratorDomainTestModule)
)]
public class SufiAbpShortLinkGeneratorApplicationTestModule : AbpModule
{

}

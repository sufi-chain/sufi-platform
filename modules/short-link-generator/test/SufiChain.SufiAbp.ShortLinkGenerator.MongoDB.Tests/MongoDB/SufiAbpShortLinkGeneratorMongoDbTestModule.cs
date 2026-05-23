using Volo.Abp.Modularity;
using SufiChain.SufiAbp.ShortLinkGenerator.MongoDB;

namespace SufiChain.SufiAbp.ShortLinkGenerator.MongoDB;

[DependsOn(
    typeof(ShortLinkGeneratorApplicationTestModule),
    typeof(ShortLinkGeneratorMongoDbModule)
)]
public class SufiAbpShortLinkGeneratorMongoDbTestModule : AbpModule
{

}

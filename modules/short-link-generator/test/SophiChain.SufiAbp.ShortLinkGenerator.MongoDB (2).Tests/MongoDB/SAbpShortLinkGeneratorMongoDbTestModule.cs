using Volo.Abp.Modularity;
using Sufihain.SufiAbp.ShortLinkGenerator.MongoDB;

namespace Sufihain.SufiAbp.ShortLinkGenerator.MongoDB;

[DependsOn(
    typeof(ShortLinkGeneratorApplicationTestModule),
    typeof(ShortLinkGeneratorMongoDbModule)
)]
public class SufiAbpShortLinkGeneratorMongoDbTestModule : AbpModule
{

}

using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.MongoDB;

[DependsOn(
    typeof(AbpMongoDbModule)
)]
public class SufiAbpMongoDbModule : AbpModule
{
}

using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Caching;

[DependsOn(
    typeof(AbpCachingModule)
)]
public class SufiAbpCachingModule : AbpModule
{
}

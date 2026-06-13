using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Caching.StackExchangeRedis;

[DependsOn(typeof(AbpCachingStackExchangeRedisModule))]
public class SufiAbpCachingStackExchangeRedisModule : AbpModule
{
}

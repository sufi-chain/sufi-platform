using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Caching;

[DependsOn(
    typeof(Volo.Abp.Caching.AbpCachingModule)
)]
public class SufiAbpCachingModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient(typeof(IDistributedCache<>), typeof(SufiAbpDistributedCache<>));
    }
}

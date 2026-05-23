using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Caching;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpDddDomainModule),
    typeof(SufiAbpCachingModule),
    typeof(SufiAbpShortLinkGeneratorDomainSharedModule)
)]
public class SufiAbpShortLinkGeneratorDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure options from configuration
        context.Services.Configure<ShortLinkGeneratorOptions>(
            context.Services.GetConfiguration().GetSection("ShortLinkGenerator"));
    }
}



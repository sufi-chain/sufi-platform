using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.Caching;
namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpCachingModule),
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



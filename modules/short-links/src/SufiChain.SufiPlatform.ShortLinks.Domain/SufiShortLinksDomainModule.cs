using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpCachingModule),
    typeof(SufiShortLinksDomainSharedModule)
)]
public class SufiShortLinksDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure options from configuration
        context.Services.Configure<ShortLinksOptions>(
            context.Services.GetConfiguration().GetSection("ShortLinks"));
    }
}
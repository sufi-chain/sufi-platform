using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Mapperly;
//using SufiChain.SufiPlatform.Ddd.Application;
using Volo.Abp.Application;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.ShortLinks;

[DependsOn(
    typeof(SufiShortLinksDomainModule),
    typeof(SufiShortLinksApplicationContractsModule),
    //typeof(SufiDddApplicationModule),
    typeof(AbpMapperlyModule),
    typeof(AbpCachingModule)
)]
public class SufiShortLinksApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // Configure ShortLinks options from appsettings.json
        Configure<ShortLinksOptions>(options =>
        {
            // Read BaseUrl from App:SelfUrl (standard configuration)
            options.BaseUrl = configuration["App:SelfUrl"] ?? "http://localhost";
            
            // Read module-specific configuration
            var shortLinkConfig = configuration.GetSection("ShortLinks");
            options.RedirectRoute = shortLinkConfig["RedirectRoute"] ?? ShortLinksConsts.DefaultRedirectRoute;
            
            if (int.TryParse(shortLinkConfig["ShortCodeLength"], out var shortCodeLength))
                options.ShortCodeLength = shortCodeLength;
                
            if (int.TryParse(shortLinkConfig["CacheExpirationMinutes"], out var cacheExpiration))
                options.CacheExpirationMinutes = cacheExpiration;
                
            if (int.TryParse(shortLinkConfig["DefaultExpirationDays"], out var defaultExpiration))
                options.DefaultExpirationDays = defaultExpiration;
        });
        
        context.Services.AddMapperlyObjectMapper<SufiShortLinksApplicationModule>();
    }
}
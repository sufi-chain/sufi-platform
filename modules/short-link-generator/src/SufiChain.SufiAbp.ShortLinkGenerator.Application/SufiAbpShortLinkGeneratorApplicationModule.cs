using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Caching;
//using SufiChain.SufiAbp.Ddd.Application;
using SufiChain.SufiAbp.Mapperly;
using Volo.Abp.Application;
using Volo.Abp.Caching;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

[DependsOn(
    typeof(SufiAbpShortLinkGeneratorDomainModule),
    typeof(SufiAbpShortLinkGeneratorApplicationContractsModule),
    //typeof(SufiAbpDddApplicationModule),
    typeof(SufiAbpMapperlyModule),
    typeof(SufiAbpCachingModule)
)]
public class SufiAbpShortLinkGeneratorApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        
        // Configure ShortLinkGenerator options from appsettings.json
        Configure<ShortLinkGeneratorOptions>(options =>
        {
            // Read BaseUrl from App:SelfUrl (standard configuration)
            options.BaseUrl = configuration["App:SelfUrl"] ?? "http://localhost";
            
            // Read module-specific configuration
            var shortLinkConfig = configuration.GetSection("ShortLinkGenerator");
            options.RedirectRoute = shortLinkConfig["RedirectRoute"] ?? ShortLinkGeneratorConsts.DefaultRedirectRoute;
            
            if (int.TryParse(shortLinkConfig["ShortCodeLength"], out var shortCodeLength))
                options.ShortCodeLength = shortCodeLength;
                
            if (int.TryParse(shortLinkConfig["CacheExpirationMinutes"], out var cacheExpiration))
                options.CacheExpirationMinutes = cacheExpiration;
                
            if (int.TryParse(shortLinkConfig["DefaultExpirationDays"], out var defaultExpiration))
                options.DefaultExpirationDays = defaultExpiration;
        });
        
        context.Services.AddMapperlyObjectMapper<SufiAbpShortLinkGeneratorApplicationModule>();
    }
}


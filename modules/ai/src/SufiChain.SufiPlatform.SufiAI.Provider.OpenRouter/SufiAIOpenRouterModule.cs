using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI.Catalog;
using Volo.Abp;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI.Provider.OpenRouter;

[DependsOn(typeof(SufiAIApplicationModule))]
public class SufiAIOpenRouterModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<OpenRouterOptions>(configuration.GetSection("SufiAI:OpenRouter"));
        var catalogBase = configuration["SufiAI:OpenRouter:CatalogBaseUrl"];
        context.Services.AddHttpClient(OpenRouterModelCatalogSource.HttpClientName, client =>
        {
            client.BaseAddress = OpenRouterModelCatalogSource.CatalogBaseAddress(catalogBase);
            client.Timeout = TimeSpan.FromSeconds(60);
        });
    }
}

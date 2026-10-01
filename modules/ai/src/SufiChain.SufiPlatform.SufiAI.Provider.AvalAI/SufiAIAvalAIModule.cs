using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI.Provider.AvalAI;

[DependsOn(typeof(SufiAIApplicationModule))]
public class SufiAIAvalAIModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(AvalAIModelCatalogSource.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://api.avalai.ir/");
            client.Timeout = TimeSpan.FromSeconds(8);
        });
    }
}

using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI.Provider.Liara;

[DependsOn(typeof(SufiAIApplicationModule))]
public class SufiAILiaraModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(LiaraModelCatalogSource.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://ai.liara.ir/");
            client.Timeout = TimeSpan.FromSeconds(8);
        });
    }
}

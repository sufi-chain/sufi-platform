using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.SufiAI.Provider.HuggingFace;

[DependsOn(typeof(SufiAIApplicationModule))]
public class SufiAIHuggingFaceModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClient(HuggingFaceModelCatalogSource.HttpClientName, client =>
        {
            client.BaseAddress = new Uri("https://router.huggingface.co/");
            client.Timeout = TimeSpan.FromSeconds(8);
        });
    }
}

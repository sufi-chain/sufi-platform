using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;

using Volo.Abp.Http.Client;
namespace SufiChain.SufiPlatform.SufiAI;

[DependsOn(
    typeof(SufiAIApplicationContractsModule),
    typeof(AbpHttpClientModule)
)]
public class SufiAIHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure dynamic C# client proxies for auto-generated controllers
        context.Services.AddHttpClientProxies(
            typeof(SufiAIApplicationContractsModule).Assembly,
            remoteServiceConfigurationName: "SufiAI"
        );
    }
}

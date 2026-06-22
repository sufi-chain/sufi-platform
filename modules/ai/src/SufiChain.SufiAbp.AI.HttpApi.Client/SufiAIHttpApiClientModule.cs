using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AI;

[DependsOn(
    typeof(SufiAIApplicationContractsModule),
    typeof(SufiAbpHttpClientModule)
)]
public class SufiAIHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure dynamic C# client proxies for auto-generated controllers
        context.Services.AddHttpClientProxies(
            typeof(SufiAIApplicationContractsModule).Assembly,
            remoteServiceConfigurationName: "AI"
        );
    }
}

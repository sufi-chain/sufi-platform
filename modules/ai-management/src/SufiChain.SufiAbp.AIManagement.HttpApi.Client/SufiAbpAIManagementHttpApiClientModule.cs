using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.AIManagement;

[DependsOn(
    typeof(SufiAbpAIManagementApplicationContractsModule),
    typeof(SufiAbpHttpClientModule)
)]
public class SufiAbpAIManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        // Configure dynamic C# client proxies for auto-generated controllers
        context.Services.AddHttpClientProxies(
            typeof(SufiAbpAIManagementApplicationContractsModule).Assembly,
            remoteServiceConfigurationName: "AIManagement"
        );
    }
}

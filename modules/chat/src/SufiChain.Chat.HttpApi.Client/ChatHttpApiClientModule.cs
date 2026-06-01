using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Http.Client;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

[DependsOn(
    typeof(ChatApplicationContractsModule),
    typeof(SufiAbpHttpClientModule)
)]
public class ChatHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(ChatApplicationContractsModule).Assembly,
            remoteServiceConfigurationName: ChatRemoteServiceConsts.RemoteServiceName
        );
    }
}

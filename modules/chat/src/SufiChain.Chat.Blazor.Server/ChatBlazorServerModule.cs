using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.Chat.Blazor.Public;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Blazor.Server.Services;
using Volo.Abp.Modularity;

namespace SufiChain.Chat.Blazor.Server;

[DependsOn(typeof(ChatBlazorModule))]
public class ChatBlazorServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpContextAccessor();
        context.Services.Replace(ServiceDescriptor.Scoped<IChatHubConnectionAccessTokenProvider, ServerChatHubConnectionAccessTokenProvider>());
    }
}

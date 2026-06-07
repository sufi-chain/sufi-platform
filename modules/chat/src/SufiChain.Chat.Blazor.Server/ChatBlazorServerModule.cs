using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.Chat.Blazor.Public;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Blazor.Server.Services;
using SufiChain.Chat.Blazor.Toolbars;
using SufiChain.Chat.Realtime;
using SufiChain.SufiAbp.UI.Toolbars;
using Volo.Abp.Modularity;

namespace SufiChain.Chat.Blazor.Server;

[DependsOn(typeof(ChatBlazorModule))]
public class ChatBlazorServerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Scoped<IChatHubConnectionAccessTokenProvider, ServerChatHubConnectionAccessTokenProvider>());

        // Validate/mint hub tickets so the server-side loopback connection conveys the authenticated user.
        context.Services.Replace(ServiceDescriptor.Singleton<IChatHubTicketProtector, DataProtectionChatHubTicketProtector>());

        Configure<ToolbarOptions>(options =>
        {
            options.Contributors.Add(new ChatToolbarContributor());
        });
    }
}

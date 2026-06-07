using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.Chat.Blazor.Public;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Blazor.Toolbars;
using SufiChain.Chat.Blazor.WebAssembly.Services;
using SufiChain.SufiAbp.UI.Toolbars;
using Volo.Abp.Modularity;

namespace SufiChain.Chat.Blazor.WebAssembly;

[DependsOn(typeof(ChatBlazorModule))]
public class ChatBlazorWebAssemblyModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Scoped<IChatHubConnectionAccessTokenProvider, WebAssemblyChatHubConnectionAccessTokenProvider>());

        Configure<ToolbarOptions>(options =>
        {
            options.Contributors.Add(new ChatToolbarContributor());
        });
    }
}

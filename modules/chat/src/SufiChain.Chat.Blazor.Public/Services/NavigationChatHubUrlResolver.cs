using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Blazor.Public.Services;

public class NavigationChatHubUrlResolver : IChatHubUrlResolver, ITransientDependency
{
    public const string DefaultHubPath = "/signalr-hubs/chat";

    protected NavigationManager NavigationManager { get; }

    protected ChatBlazorPublicOptions Options { get; }

    public NavigationChatHubUrlResolver(
        NavigationManager navigationManager,
        IOptions<ChatBlazorPublicOptions> options)
    {
        NavigationManager = navigationManager;
        Options = options.Value;
    }

    public string GetHubUrl()
    {
        var hubPath = string.IsNullOrWhiteSpace(Options.HubPath)
            ? DefaultHubPath
            : Options.HubPath;

        return NavigationManager.ToAbsoluteUri(hubPath).ToString();
    }
}

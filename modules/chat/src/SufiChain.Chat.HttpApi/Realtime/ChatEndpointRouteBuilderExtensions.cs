using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace SufiChain.Chat.Realtime;

public static class ChatEndpointRouteBuilderExtensions
{
    public const string ChatHubRoute = "/signalr-hubs/chat";

    public static IEndpointRouteBuilder MapChatHub(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<ChatHub>(ChatHubRoute);
        return endpoints;
    }
}

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using SufiChain.Chat.Blazor.Public.Services;

namespace SufiChain.Chat.Blazor.Server.Services;

/// <summary>
/// Server-side access token provider for authenticated SignalR hub connections.
/// </summary>
public class ServerChatHubConnectionAccessTokenProvider : IChatHubConnectionAccessTokenProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerChatHubConnectionAccessTokenProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return await httpContext.GetTokenAsync("access_token");
    }
}

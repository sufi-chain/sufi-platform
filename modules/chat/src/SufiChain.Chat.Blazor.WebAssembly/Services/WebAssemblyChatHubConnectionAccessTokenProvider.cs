using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using SufiChain.Chat.Blazor.Public.Services;

namespace SufiChain.Chat.Blazor.WebAssembly.Services;

/// <summary>
/// WebAssembly access token provider for authenticated SignalR hub connections.
/// </summary>
public class WebAssemblyChatHubConnectionAccessTokenProvider : IChatHubConnectionAccessTokenProvider
{
    private readonly IAccessTokenProvider _accessTokenProvider;

    public WebAssemblyChatHubConnectionAccessTokenProvider(IAccessTokenProvider accessTokenProvider)
    {
        _accessTokenProvider = accessTokenProvider;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var tokenResult = await _accessTokenProvider.RequestAccessToken();
        if (tokenResult.TryGetToken(out var token))
        {
            return token.Value;
        }

        return null;
    }
}

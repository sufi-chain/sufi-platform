using Microsoft.AspNetCore.Components.Authorization;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Realtime;

namespace SufiChain.Chat.Blazor.Server.Services;

/// <summary>
/// Server-side access token provider for SignalR hub connections.
/// <para>
/// On Blazor Server the hub connection is opened from the server process during an interactive circuit,
/// where <c>IHttpContextAccessor.HttpContext</c> is null and there is no OAuth access token (cookie auth).
/// We therefore read the authenticated user from the circuit's <see cref="AuthenticationStateProvider"/>
/// and mint a short-lived protected ticket that the hub validates server-side.
/// </para>
/// </summary>
public class ServerChatHubConnectionAccessTokenProvider : IChatHubConnectionAccessTokenProvider
{
    protected AuthenticationStateProvider AuthenticationStateProvider { get; }

    protected IChatHubTicketProtector TicketProtector { get; }

    public ServerChatHubConnectionAccessTokenProvider(
        AuthenticationStateProvider authenticationStateProvider,
        IChatHubTicketProtector ticketProtector)
    {
        AuthenticationStateProvider = authenticationStateProvider;
        TicketProtector = ticketProtector;
    }

    public virtual async Task<string?> GetAccessTokenAsync()
    {
        var state = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (state.User.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        return TicketProtector.Protect(state.User);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SufiChain.Chat.Realtime;

[AllowAnonymous]
public class ChatHub : Hub<IChatHubClient>
{
    protected IChatRealtimeAccessChecker AccessChecker { get; }

    public ChatHub(IChatRealtimeAccessChecker accessChecker)
    {
        AccessChecker = accessChecker;
    }

    public virtual async Task JoinSessionGroupAsync(Guid sessionId, string? anonymousVisitorId = null)
    {
        if (!await AccessChecker.CanJoinSessionAsync(sessionId, anonymousVisitorId))
        {
            throw new HubException("Unauthorized chat session access.");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, ChatRealtimeGroups.Session(sessionId));
    }

    public virtual Task LeaveSessionGroupAsync(Guid sessionId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, ChatRealtimeGroups.Session(sessionId));
    }
}

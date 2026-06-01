using Microsoft.AspNetCore.SignalR;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Realtime;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IChatRealtimeNotifier))]
public class SignalRChatRealtimeNotifier : IChatRealtimeNotifier, ITransientDependency
{
    protected IHubContext<ChatHub, IChatHubClient> HubContext { get; }

    public SignalRChatRealtimeNotifier(IHubContext<ChatHub, IChatHubClient> hubContext)
    {
        HubContext = hubContext;
    }

    public virtual Task NotifyMessageSentAsync(ChatMessageDto message)
    {
        return HubContext.Clients
            .Group(ChatRealtimeGroups.Session(message.SessionId))
            .MessageReceived(message);
    }

    public virtual Task NotifySessionUpdatedAsync(ChatSessionDto session)
    {
        return HubContext.Clients
            .Group(ChatRealtimeGroups.Session(session.Id))
            .SessionUpdated(session);
    }

    public virtual Task NotifyUsageLimitExceededAsync(Guid sessionId, ChatUsageCheckResultDto result)
    {
        return HubContext.Clients
            .Group(ChatRealtimeGroups.Session(sessionId))
            .UsageLimitExceeded(result);
    }
}

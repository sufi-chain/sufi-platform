using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Realtime;

/// <summary>
/// No-op realtime notifier used when SignalR hub wiring is not present.
/// </summary>
public class NullChatRealtimeNotifier : IChatRealtimeNotifier, ISingletonDependency
{
    public Task NotifyMessageSentAsync(ChatMessageDto message)
    {
        return Task.CompletedTask;
    }

    public Task NotifySessionUpdatedAsync(ChatSessionDto session)
    {
        return Task.CompletedTask;
    }

    public Task NotifyUsageLimitExceededAsync(Guid sessionId, ChatUsageCheckResultDto result)
    {
        return Task.CompletedTask;
    }
}

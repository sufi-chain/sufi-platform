using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;

namespace SufiChain.Chat.Realtime;

public interface IChatRealtimeNotifier
{
    Task NotifyMessageSentAsync(ChatMessageDto message);

    Task NotifySessionUpdatedAsync(ChatSessionDto session);

    Task NotifyUsageLimitExceededAsync(Guid sessionId, ChatUsageCheckResultDto result);
}

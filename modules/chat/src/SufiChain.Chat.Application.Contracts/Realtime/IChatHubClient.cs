using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;

namespace SufiChain.Chat.Realtime;

public interface IChatHubClient
{
    Task MessageReceived(ChatMessageDto message);

    Task SessionUpdated(ChatSessionDto session);

    Task UsageLimitExceeded(ChatUsageCheckResultDto result);
}

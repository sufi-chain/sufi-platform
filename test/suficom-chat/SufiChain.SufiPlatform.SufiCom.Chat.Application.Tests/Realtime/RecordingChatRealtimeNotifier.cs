using SufiChain.SufiPlatform.SufiCom.Chat.Messages;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Realtime;

public class RecordingChatRealtimeNotifier : IChatRealtimeNotifier
{
    public List<ChatMessageDto> Messages { get; } = new();

    public Task NotifyMessageSentAsync(ChatMessageDto message)
    {
        Messages.Add(message);
        return Task.CompletedTask;
    }

    public Task NotifySessionUpdatedAsync(ChatSessionDto session) => Task.CompletedTask;

    public Task NotifyUsageLimitExceededAsync(Guid sessionId, ChatUsageCheckResultDto result) => Task.CompletedTask;

    public Task NotifyAssistantProgressAsync(ChatAssistantProgressDto progress) => Task.CompletedTask;

    public Task NotifyHooshvareTurnProgressAsync(Guid userId, ChatAssistantProgressDto progress) => Task.CompletedTask;

    public Task NotifyOperatorQueueChangedAsync() => Task.CompletedTask;
}

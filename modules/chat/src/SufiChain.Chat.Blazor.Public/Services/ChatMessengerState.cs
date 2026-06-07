using SufiChain.Chat.Composer;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;

namespace SufiChain.Chat.Blazor.Public.Services;

public enum ChatMessengerMobileView
{
    ConversationList,
    Timeline
}

public sealed class ChatMessengerSignupRequiredState
{
    public string LocalizationKey { get; init; } = "Chat:AuthenticationRequired";
}

public class ChatMessengerState
{
    public List<ChatSessionListDto> Sessions { get; set; } = new();

    public List<ChatMessageDto> Messages { get; set; } = new();

    public Guid? SelectedSessionId { get; set; }

    public ChatSessionDto? SelectedSession { get; set; }

    public string DraftMessage { get; set; } = string.Empty;

    public List<Guid> DraftAttachmentFileIds { get; set; } = new();

    public string? DraftMetadataJson { get; set; }

    public ChatComposerCapabilitiesDto? ComposerCapabilities { get; set; }

    public bool IsLoadingSessions { get; set; }

    public bool IsLoadingMessages { get; set; }

    public bool IsSendingMessage { get; set; }
    public bool IsWaitingForAiResponse { get; set; }

    public ChatMessengerMobileView MobileView { get; set; } = ChatMessengerMobileView.ConversationList;

    public ChatMessengerSignupRequiredState? SignupRequired { get; private set; }

    public event Action? StateChanged;

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    public void SelectSession(Guid sessionId)
    {
        SelectedSessionId = sessionId;
        MobileView = ChatMessengerMobileView.Timeline;
        NotifyStateChanged();
    }

    public void ShowConversationList()
    {
        MobileView = ChatMessengerMobileView.ConversationList;
        NotifyStateChanged();
    }

    public void ClearSignupRequired()
    {
        SignupRequired = null;
        NotifyStateChanged();
    }

    public void ClearDraft()
    {
        DraftMessage = string.Empty;
        DraftAttachmentFileIds.Clear();
        DraftMetadataJson = null;
        NotifyStateChanged();
    }

    public void ApplyUsageLimit(ChatUsageCheckResultDto result)
    {
        if (result.Action == LimitExceededAction.RequireAuthentication || result.RequiresAuthentication)
        {
            SignupRequired = new ChatMessengerSignupRequiredState
            {
                LocalizationKey = result.LocalizationKey ?? "Chat:AuthenticationRequired"
            };
        }

        NotifyStateChanged();
    }
}

using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Usage;

namespace SufiChain.Chat.Blazor.Public;

public enum ChatMessengerMobileView
{
    ConversationList,
    Timeline
}

public class ChatSignupRequiredState
{
    public string LocalizationKey { get; set; } = "Chat:AuthenticationRequired";

    public string? SignInUrl { get; set; }
}

/// <summary>
/// Shared messenger UI state for public chat surfaces.
/// </summary>
public class ChatMessengerState
{
    public Guid? SelectedSessionId { get; set; }

    public ChatMessengerMobileView MobileView { get; set; } = ChatMessengerMobileView.ConversationList;

    public bool IsContextPanelOpen { get; set; } = true;

    public List<ChatSessionListDto> Sessions { get; set; } = new();

    public List<ChatMessageDto> Messages { get; set; } = new();

    public ChatSessionDto? SelectedSession { get; set; }

    public ChatSignupRequiredState? SignupRequired { get; set; }

    public bool IsLoadingSessions { get; set; }

    public bool IsLoadingMessages { get; set; }

    public bool IsSendingMessage { get; set; }

    public string DraftMessage { get; set; } = string.Empty;

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

    public void ApplyUsageLimit(ChatUsageCheckResultDto result)
    {
        if (result.RequiresAuthentication || result.Action == LimitExceededAction.RequireAuthentication)
        {
            SignupRequired = new ChatSignupRequiredState
            {
                LocalizationKey = string.IsNullOrWhiteSpace(result.LocalizationKey)
                    ? "Chat:AuthenticationRequired"
                    : result.LocalizationKey
            };
        }

        NotifyStateChanged();
    }

    public void ClearSignupRequired()
    {
        SignupRequired = null;
        NotifyStateChanged();
    }
}

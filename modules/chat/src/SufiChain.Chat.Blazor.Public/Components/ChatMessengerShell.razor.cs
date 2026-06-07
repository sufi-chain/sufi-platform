using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;
using SufiChain.Chat.Blazor.Public.Services;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatMessengerShell : ChatPublicComponentBase
{
    [Parameter]
    public ChatMessengerState State { get; set; } = default!;

    [Parameter]
    public bool ShowContextPanel { get; set; } = true;

    [Parameter]
    public string? SignInUrl { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public EventCallback<Guid> OnSessionSelected { get; set; }

    [Parameter]
    public EventCallback<ChatComposerSendRequest> OnSendMessage { get; set; }

    [Parameter]
    public RenderFragment? ConversationListSections { get; set; }

    [Parameter]
    public RenderFragment? ListHeader { get; set; }

    [Parameter]
    public RenderFragment? ListFooter { get; set; }

    [Parameter]
    public RenderFragment<ChatSessionListDto>? ItemTemplate { get; set; }

    [Parameter]
    public RenderFragment<ChatSessionListDto>? ItemTrailing { get; set; }

    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    [Parameter]
    public RenderFragment? ContextPanel { get; set; }

    [Parameter]
    public RenderFragment? ContextPanelHeader { get; set; }

    [Parameter]
    public RenderFragment? ComposerToolbar { get; set; }

    [Parameter]
    public RenderFragment<ChatMessageDto>? MessageActions { get; set; }

    [Parameter]
    public RenderFragment? EmptySelection { get; set; }

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    protected bool IsMobileTimelineView =>
        State.MobileView == ChatMessengerMobileView.Timeline;

    protected bool IsSignupRequired => State.SignupRequired != null;

    protected string? SignupRequiredLocalizationKey => State.SignupRequired?.LocalizationKey;

    protected bool IsComposerDisabled =>
        State.IsSendingMessage ||
        State.SelectedSession?.Status == ChatSessionStatus.Closed;

    protected override void OnInitialized()
    {
        State.StateChanged += OnStateChanged;
    }

    protected string GetShellClass()
    {
        return ShowContextPanel
            ? "chat-messenger-shell--three-column"
            : "chat-messenger-shell--two-column";
    }

    protected string GetListPaneClass()
    {
        if (State.MobileView == ChatMessengerMobileView.Timeline)
        {
            return "chat-messenger-shell__pane--hidden-mobile";
        }

        return string.Empty;
    }

    protected string GetMainPaneClass()
    {
        if (State.MobileView == ChatMessengerMobileView.ConversationList)
        {
            return "chat-messenger-shell__pane--hidden-mobile";
        }

        return string.Empty;
    }

    protected string GetContextPaneClass()
    {
        return "chat-messenger-shell__pane--hidden-mobile";
    }

    protected async Task OnSessionSelectedAsync(Guid sessionId)
    {
        State.SelectSession(sessionId);
        await OnSessionSelected.InvokeAsync(sessionId);
    }

    protected Task OnBackAsync()
    {
        State.ShowConversationList();
        return Task.CompletedTask;
    }

    protected Task OnDraftChangedAsync(string draft)
    {
        State.DraftMessage = draft;
        return Task.CompletedTask;
    }

    protected Task OnDraftAttachmentsChangedAsync(List<Guid> attachmentFileIds)
    {
        State.DraftAttachmentFileIds = attachmentFileIds;
        return Task.CompletedTask;
    }

    protected Task OnDraftMetadataChangedAsync(string? metadataJson)
    {
        State.DraftMetadataJson = metadataJson;
        return Task.CompletedTask;
    }

    protected async Task OnSendAsync(ChatComposerSendRequest request)
    {
        await OnSendMessage.InvokeAsync(request);
    }

    protected void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            State.StateChanged -= OnStateChanged;
        }

        base.Dispose(disposing);
    }
}

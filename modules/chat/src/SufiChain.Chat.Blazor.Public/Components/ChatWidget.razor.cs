using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Blazor.Public.Services;
using SufiChain.Chat.Composer;
using SufiChain.Chat.Messages;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatWidget : ChatPublicComponentBase
{
    [Parameter]
    public ChatMessengerState State { get; set; } = default!;

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
    public RenderFragment? ComposerToolbar { get; set; }

    [Parameter]
    public RenderFragment<ChatMessageDto>? MessageActions { get; set; }

    [Parameter]
    public RenderFragment? EmptySelection { get; set; }

    [Parameter]
    public RenderFragment? EmptyTemplate { get; set; }

    private bool _isOpen;

    protected override void OnInitialized()
    {
        State.StateChanged += OnStateChanged;
    }

    protected void ToggleWidget()
    {
        _isOpen = !_isOpen;
    }

    protected void CloseWidget()
    {
        _isOpen = false;
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

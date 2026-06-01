using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatSessionHeader : ChatPublicComponentBase
{
    [Parameter]
    public ChatSessionDto? Session { get; set; }

    [Parameter]
    public bool ShowBackButton { get; set; }

    [Parameter]
    public EventCallback OnBack { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? HeaderActions { get; set; }

    protected string GetSessionTitle()
    {
        if (Session == null)
        {
            return L["Messenger:Conversations"];
        }

        if (!string.IsNullOrWhiteSpace(Session.Title))
        {
            return Session.Title;
        }

        return Session.ConversationKind.ToString();
    }

    protected async Task OnBackInternal()
    {
        await OnBack.InvokeAsync();
    }
}

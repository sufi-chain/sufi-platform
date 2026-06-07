using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Blazor.Public;
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

    protected string GetSessionTitle() =>
        ChatSessionUiTitle.GetTitle(Session, key => L[key], CurrentUser.Id);

    protected async Task OnBackInternal()
    {
        await OnBack.InvokeAsync();
    }
}

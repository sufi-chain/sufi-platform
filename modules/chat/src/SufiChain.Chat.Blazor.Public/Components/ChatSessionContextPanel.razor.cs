using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatSessionContextPanel : ChatPublicComponentBase
{
    [Parameter]
    public ChatSessionDto? Session { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment? ContextPanel { get; set; }

    [Parameter]
    public RenderFragment? PanelHeader { get; set; }
}

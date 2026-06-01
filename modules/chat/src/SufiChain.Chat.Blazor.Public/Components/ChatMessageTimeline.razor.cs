using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Messages;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatMessageTimeline : ChatPublicComponentBase
{
    [Parameter]
    public IEnumerable<ChatMessageDto> Messages { get; set; } = Enumerable.Empty<ChatMessageDto>();

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public RenderFragment<ChatMessageDto>? MessageActions { get; set; }
}

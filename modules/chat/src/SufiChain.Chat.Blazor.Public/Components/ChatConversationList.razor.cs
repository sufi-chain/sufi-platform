using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Blazor.Public;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Public.Components;

public partial class ChatConversationList : ChatPublicComponentBase
{
    [Parameter]
    public IEnumerable<ChatSessionListDto> Sessions { get; set; } = Enumerable.Empty<ChatSessionListDto>();

    [Parameter]
    public Guid? SelectedSessionId { get; set; }

    [Parameter]
    public EventCallback<Guid> SelectedSessionIdChanged { get; set; }

    [Parameter]
    public bool IsLoading { get; set; }

    [Parameter]
    public string? Class { get; set; }

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
    public RenderFragment? EmptyTemplate { get; set; }

    protected bool IsSelected(ChatSessionListDto session)
    {
        return SelectedSessionId == session.Id;
    }

    protected async Task SelectSessionAsync(ChatSessionListDto session)
    {
        if (SelectedSessionId == session.Id)
        {
            return;
        }

        SelectedSessionId = session.Id;
        await SelectedSessionIdChanged.InvokeAsync(session.Id);
    }

    protected string GetSessionTitle(ChatSessionListDto session) =>
        ChatSessionUiTitle.GetTitle(session, key => L[key]);
}

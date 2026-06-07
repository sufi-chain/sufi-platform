using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Components;

/// <summary>
/// Operator inbox administration filters and refresh for the messenger context rail and mobile list header.
/// </summary>
public partial class ChatOperatorInboxAdminActions : ChatComponentBase
{
    protected static IReadOnlyList<ConversationKind> AllConversationKinds { get; } =
        Enum.GetValues<ConversationKind>();

    protected static IReadOnlyList<AccessMode> AllAccessModes { get; } =
        Enum.GetValues<AccessMode>();

    [Parameter]
    public ChatSessionStatus? StatusFilter { get; set; }

    [Parameter]
    public EventCallback<ChatSessionStatus?> StatusFilterChanged { get; set; }

    [Parameter]
    public ConversationKind? ConversationKindFilter { get; set; }

    [Parameter]
    public EventCallback<ConversationKind?> ConversationKindFilterChanged { get; set; }

    [Parameter]
    public AccessMode? AccessModeFilter { get; set; }

    [Parameter]
    public EventCallback<AccessMode?> AccessModeFilterChanged { get; set; }

    [Parameter]
    public EventCallback OnRefresh { get; set; }

    [Parameter]
    public bool IsRefreshDisabled { get; set; }

    [Parameter]
    public string? Class { get; set; }
}

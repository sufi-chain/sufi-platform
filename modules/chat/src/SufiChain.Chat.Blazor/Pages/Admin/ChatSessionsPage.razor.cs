using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using SufiChain.SufiBlazor.Components;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.Chat.Blazor.Pages.Admin;

[Authorize(ChatPermissions.Sessions.Default)]
public partial class ChatSessionsPage : ChatComponentBase
{
    [Inject]
    protected IChatSessionAppService SessionAppService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected IReadOnlyList<ChatSessionListDto> Sessions { get; set; } = Array.Empty<ChatSessionListDto>();

    protected int TotalCount { get; set; }

    protected int PageIndex { get; set; }

    protected int PageSize { get; set; } = 20;

    protected string CurrentSorting { get; set; } = string.Empty;

    protected bool IsLoading { get; set; }

    protected ChatSessionStatus? StatusFilter { get; set; }

    protected ConversationKind? ConversationKindFilter { get; set; }

    protected AccessMode? AccessModeFilter { get; set; }

    protected static IReadOnlyList<ConversationKind> AllConversationKinds { get; } =
        Enum.GetValues<ConversationKind>();

    protected static IReadOnlyList<AccessMode> AllAccessModes { get; } =
        Enum.GetValues<AccessMode>();

    protected override async Task OnInitializedAsync()
    {
        await LoadSessionsAsync();
    }

    protected virtual async Task LoadSessionsAsync()
    {
        IsLoading = true;
        try
        {
            var result = await SessionAppService.GetListAsync(new GetChatSessionListInput
            {
                MaxResultCount = PageSize,
                SkipCount = PageIndex * PageSize,
                Sorting = string.IsNullOrWhiteSpace(CurrentSorting)
                    ? nameof(ChatSessionListDto.LastMessageTime) + " DESC"
                    : CurrentSorting,
                Status = StatusFilter,
                ConversationKind = ConversationKindFilter,
                AccessMode = AccessModeFilter
            });

            Sessions = result.Items;
            TotalCount = (int)result.TotalCount;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual Task RefreshAsync()
    {
        PageIndex = 0;
        return LoadSessionsAsync();
    }

    protected virtual Task OnStatusFilterChangedAsync(ChatSessionStatus? value)
    {
        StatusFilter = value;
        PageIndex = 0;
        return LoadSessionsAsync();
    }

    protected virtual Task OnConversationKindFilterChangedAsync(ConversationKind? value)
    {
        ConversationKindFilter = value;
        PageIndex = 0;
        return LoadSessionsAsync();
    }

    protected virtual Task OnAccessModeFilterChangedAsync(AccessMode? value)
    {
        AccessModeFilter = value;
        PageIndex = 0;
        return LoadSessionsAsync();
    }

    protected virtual async Task OnPageIndexChangedAsync(int pageIndex)
    {
        PageIndex = pageIndex;
        await LoadSessionsAsync();
    }

    protected virtual async Task OnSortChangedAsync(SbSort? sort)
    {
        CurrentSorting = sort == null || string.IsNullOrWhiteSpace(sort.Field)
            ? string.Empty
            : sort.Field + (sort.Direction == SbSortDirection.Descending ? " DESC" : string.Empty);

        PageIndex = 0;
        await LoadSessionsAsync();
    }

    protected virtual void OpenSession(Guid sessionId)
    {
        NavigationManager.NavigateTo($"/admin/chat/sessions/{sessionId}");
    }

    protected static SbColor GetStatusChipColor(ChatSessionStatus status) => status switch
    {
        ChatSessionStatus.Open => SbColor.Success,
        ChatSessionStatus.Closed => SbColor.Muted,
        _ => SbColor.Default
    };

    protected static SbColor GetAccessModeChipColor(AccessMode mode) => mode switch
    {
        AccessMode.PublicAnonymous => SbColor.Warning,
        AccessMode.PublicAuthenticated => SbColor.Info,
        AccessMode.Internal => SbColor.Primary,
        _ => SbColor.Default
    };
}

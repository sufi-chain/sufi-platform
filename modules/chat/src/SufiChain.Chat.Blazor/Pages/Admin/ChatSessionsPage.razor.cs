using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
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
                    : CurrentSorting
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
}

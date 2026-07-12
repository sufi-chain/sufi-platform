using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Users;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Users.Blazor.Public.Components;

public partial class SufiUserMultiSelector : UsersPublicComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadUsers = "load-users";
    }

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public IReadOnlyCollection<Guid> UserIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public EventCallback<IReadOnlyList<Guid>> UserIdsChanged { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyList<UserLookupDto>> SelectedUsersChanged { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyList<UserLookupDto>> SelectionConfirmed { get; set; }

    [Parameter]
    public IReadOnlyCollection<Guid> ExcludedUserIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    private IUserLookupAppService UserLookupAppService => LazyGetRequiredService(ref _userLookupAppService);
    private IUserLookupAppService? _userLookupAppService;

    private SufiUserSelectorUserGrid? _userGridRef;
    private List<UserLookupDto> _selectedUsers = new();
    private HashSet<string> _selectedKeys = new();
    private HashSet<Guid> _excludedUserIds = new();
    private string? _filter;
    private int _pageIndex;
    private int _pageSize = 10;
    private long _totalCount;
    private bool _wasOpen;

    protected string DialogTitle => string.IsNullOrWhiteSpace(Title) ? L["SelectUsers"] : Title;

    protected override async Task OnParametersSetAsync()
    {
        _excludedUserIds = ExcludedUserIds.ToHashSet();

        if (Open && !_wasOpen)
        {
            _filter = null;
            _pageIndex = 0;
            await SyncSelectionFromUserIdsAsync();
            await RefreshUsersGridAsync();
        }

        _wasOpen = Open;
    }

    public virtual async Task OpenAsync()
    {
        await OpenChanged.InvokeAsync(true);
    }

    public virtual async Task CloseAsync()
    {
        await OpenChanged.InvokeAsync(false);
    }

    private async Task SyncSelectionFromUserIdsAsync()
    {
        _selectedUsers.Clear();
        _selectedKeys.Clear();

        foreach (var userId in UserIds.Distinct())
        {
            if (_excludedUserIds.Contains(userId))
            {
                continue;
            }

            try
            {
                var user = await UserLookupAppService.GetAsync(userId);
                _selectedUsers.Add(user);
                _selectedKeys.Add(user.Id.ToString());
            }
            catch
            {
                // Ignore stale ids that no longer resolve.
            }
        }
    }

    private async Task<SbDataResponse<UserLookupDto>> LoadUsersDataAsync(SbDataRequest request)
    {
        var result = await UserLookupAppService.SearchAsync(new UserLookupSearchInput
        {
            Filter = _filter,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        });

        var items = result.Items
            .Where(user => !_excludedUserIds.Contains(user.Id))
            .ToList();

        _totalCount = result.TotalCount;
        return new SbDataResponse<UserLookupDto>(items, result.TotalCount);
    }

    private Task RefreshUsersGridAsync()
    {
        return ExecuteWithLoadingAsync(
            () => _userGridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadUsers);
    }

    private async Task OnFilterChangedAsync()
    {
        _pageIndex = 0;
        await RefreshUsersGridAsync();
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await RefreshUsersGridAsync();
    }

    private async Task OnSelectedKeysChangedAsync(IReadOnlySet<string> keys)
    {
        _selectedKeys = keys.ToHashSet();

        var selectedIds = keys
            .Select(key => Guid.TryParse(key, out var id) ? id : Guid.Empty)
            .Where(id => id != Guid.Empty)
            .ToHashSet();

        var retained = _selectedUsers
            .Where(user => selectedIds.Contains(user.Id))
            .ToList();

        foreach (var userId in selectedIds)
        {
            if (retained.Any(existing => existing.Id == userId))
            {
                continue;
            }

            try
            {
                retained.Add(await UserLookupAppService.GetAsync(userId));
            }
            catch
            {
                // Ignore stale ids that no longer resolve.
            }
        }

        _selectedUsers = retained
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.UserName)
            .ToList();

        await UserIdsChanged.InvokeAsync(_selectedUsers.Select(user => user.Id).ToList());
        await SelectedUsersChanged.InvokeAsync(_selectedUsers);
    }

    private async Task ConfirmAsync()
    {
        if (_selectedUsers.Count == 0)
        {
            return;
        }

        await SelectionConfirmed.InvokeAsync(_selectedUsers);
        await UserIdsChanged.InvokeAsync(_selectedUsers.Select(user => user.Id).ToList());
        await SelectedUsersChanged.InvokeAsync(_selectedUsers);
        await CloseAsync();
    }

    private async Task OnDialogOpenChangedAsync(bool isOpen)
    {
        if (!isOpen)
        {
            await CloseAsync();
        }
    }
}

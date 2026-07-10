using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Users;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiAbp.Users.Blazor.Public.Components;

public partial class SufiUserSelector : UsersPublicComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadUsers = "load-users";
    }

    [Parameter]
    public Guid? UserId { get; set; }

    [Parameter]
    public EventCallback<Guid?> UserIdChanged { get; set; }

    [Parameter]
    public EventCallback<UserLookupDto?> SelectedUserChanged { get; set; }

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public bool AllowClear { get; set; } = true;

    private IUserLookupAppService UserLookupAppService => LazyGetRequiredService(ref _userLookupAppService);
    private IUserLookupAppService? _userLookupAppService;

    private SufiUserSelectorUserGrid? _userGridRef;
    private bool _showDialog;
    private UserLookupDto? _selectedUser;
    private UserLookupDto? _dialogSelection;
    private HashSet<string> _selectedKeys = new();
    private string? _filter;
    private int _pageIndex;
    private int _pageSize = 10;
    private long _totalCount;
    private Guid? _loadedUserId;

    protected bool HasSelectedUser => _selectedUser is not null;

    protected bool HasDialogSelection => _dialogSelection is not null;

    protected string SelectedUserSummary => _selectedUser is null
        ? string.Empty
        : $"{_selectedUser.DisplayName} ({_selectedUser.UserName})";

    protected override async Task OnParametersSetAsync()
    {
        if (UserId.HasValue && UserId != _loadedUserId)
        {
            await LoadSelectedUserAsync(UserId.Value);
        }
        else if (!UserId.HasValue && _loadedUserId.HasValue)
        {
            _selectedUser = null;
            _loadedUserId = null;
        }
    }

    public virtual async Task OpenAsync()
    {
        _dialogSelection = _selectedUser;
        _selectedKeys = _selectedUser == null
            ? new HashSet<string>()
            : new HashSet<string> { _selectedUser.Id.ToString() };
        _filter = null;
        _pageIndex = 0;
        _showDialog = true;
        await RefreshUsersGridAsync();
    }

    public virtual Task CloseAsync()
    {
        _showDialog = false;
        return Task.CompletedTask;
    }

    private async Task LoadSelectedUserAsync(Guid userId)
    {
        try
        {
            _selectedUser = await UserLookupAppService.GetAsync(userId);
            _loadedUserId = userId;
        }
        catch
        {
            _selectedUser = null;
            _loadedUserId = userId;
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

        _totalCount = result.TotalCount;
        return new SbDataResponse<UserLookupDto>(result.Items, result.TotalCount);
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

    private Task OnRowClickedAsync(UserLookupDto item)
    {
        _dialogSelection = item;
        _selectedKeys = new HashSet<string> { item.Id.ToString() };
        return Task.CompletedTask;
    }

    private Task OnSelectedKeysChangedAsync(IReadOnlySet<string> keys)
    {
        _selectedKeys = keys.ToHashSet();
        var selectedId = keys.FirstOrDefault();
        _dialogSelection = string.IsNullOrEmpty(selectedId) || _dialogSelection?.Id.ToString() == selectedId
            ? _dialogSelection
            : null;
        return Task.CompletedTask;
    }

    private async Task ConfirmAsync()
    {
        if (_dialogSelection == null)
        {
            return;
        }

        _selectedUser = _dialogSelection;
        _loadedUserId = _selectedUser.Id;
        await UserIdChanged.InvokeAsync(_selectedUser.Id);
        await SelectedUserChanged.InvokeAsync(_selectedUser);
        await CloseAsync();
    }

    private async Task ClearAsync()
    {
        _selectedUser = null;
        _loadedUserId = null;
        await UserIdChanged.InvokeAsync(null);
        await SelectedUserChanged.InvokeAsync(null);
    }

    private async Task OnDialogOpenChangedAsync(bool isOpen)
    {
        if (!isOpen)
        {
            await CloseAsync();
        }
    }
}

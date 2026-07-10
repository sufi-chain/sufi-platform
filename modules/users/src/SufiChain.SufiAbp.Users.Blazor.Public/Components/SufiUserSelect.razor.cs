using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Users;

namespace SufiChain.SufiAbp.Users.Blazor.Public.Components;

public partial class SufiUserSelect : SufiUserLookupInlineComponentBase
{
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

    [Parameter]
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;

    private UserLookupDto? _selectedUser;
    private List<UserLookupDto> _lastSearchResults = new();
    private Guid? _loadedUserId;

    protected override async Task OnParametersSetAsync()
    {
        if (UserId.HasValue && UserId != _loadedUserId)
        {
            await EnsureSelectedUserDisplayedAsync(UserId.Value);
        }
        else if (!UserId.HasValue && _loadedUserId.HasValue)
        {
            _selectedUser = null;
            _loadedUserId = null;
        }
    }

    private async Task EnsureSelectedUserDisplayedAsync(Guid userId)
    {
        if (_selectedUser?.Id == userId)
        {
            _loadedUserId = userId;
            return;
        }

        _selectedUser = await TryGetUserAsync(userId);
        _loadedUserId = userId;
    }

    private async Task<IEnumerable<UserLookupDto>> SearchUsersForSelectAsync(string filter)
    {
        _lastSearchResults = await SearchUsersAsync(filter, MaxResultCount);
        return _lastSearchResults;
    }

    private async Task OnSelectedUserChangedAsync(UserLookupDto? user)
    {
        _selectedUser = user;
        _loadedUserId = user?.Id;
        await UserIdChanged.InvokeAsync(user?.Id);
        await SelectedUserChanged.InvokeAsync(user);
    }
}

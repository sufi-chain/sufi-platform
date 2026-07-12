using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Users;

namespace SufiChain.SufiPlatform.Users.Blazor.Public.Components;

public partial class SufiUserMultiSelect : SufiUserLookupInlineComponentBase
{
    [Parameter]
    public IReadOnlyCollection<Guid> UserIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public EventCallback<IReadOnlyList<Guid>> UserIdsChanged { get; set; }

    [Parameter]
    public EventCallback<IReadOnlyList<UserLookupDto>> SelectedUsersChanged { get; set; }

    [Parameter]
    public IReadOnlyCollection<Guid> ExcludedUserIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public string? Label { get; set; }

    [Parameter]
    public bool Disabled { get; set; }

    [Parameter]
    public int? MaxSelected { get; set; }

    [Parameter]
    public int MaxResultCount { get; set; } = DefaultMaxResultCount;

    private List<UserLookupDto> _displayItems = new();
    private List<UserLookupDto> _selectedUsers = new();
    private List<Guid> _selectedUserIds = new();
    private HashSet<Guid> _excludedUserIds = new();
    private bool _initialized;

    protected override async Task OnInitializedAsync()
    {
        _excludedUserIds = ExcludedUserIds.ToHashSet();
        await LoadDisplayItemsAsync();
        await SyncSelectionFromUserIdsAsync();
        _initialized = true;
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized)
        {
            return;
        }

        _excludedUserIds = ExcludedUserIds.ToHashSet();

        var incomingIds = UserIds.Distinct().ToList();
        if (!incomingIds.SequenceEqual(_selectedUserIds))
        {
            await SyncSelectionFromUserIdsAsync();
        }
    }

    private async Task SyncSelectionFromUserIdsAsync()
    {
        _selectedUsers.Clear();
        _selectedUserIds.Clear();

        foreach (var userId in UserIds.Distinct())
        {
            if (_excludedUserIds.Contains(userId))
            {
                continue;
            }

            var user = _selectedUsers.FirstOrDefault(existing => existing.Id == userId)
                ?? _displayItems.FirstOrDefault(existing => existing.Id == userId)
                ?? await TryGetUserAsync(userId);

            if (user == null)
            {
                continue;
            }

            _selectedUsers.Add(user);
            _selectedUserIds.Add(user.Id);
        }

        _displayItems = _selectedUsers.ToList();
    }

    private async Task LoadDisplayItemsAsync()
    {
        var users = await SearchUsersAsync(null, MaxResultCount);
        _displayItems = users
            .Where(user => !_excludedUserIds.Contains(user.Id))
            .ToList();
        MergeUsers(_displayItems, _selectedUsers);
    }

    private async Task OnValuesChangedAsync(IReadOnlyList<Guid> values)
    {
        var selectedIds = values
            .Where(id => !_excludedUserIds.Contains(id))
            .Distinct()
            .ToList();

        if (MaxSelected.HasValue && selectedIds.Count > MaxSelected.Value)
        {
            selectedIds = selectedIds.Take(MaxSelected.Value).ToList();
        }

        var retained = _selectedUsers
            .Where(user => selectedIds.Contains(user.Id))
            .ToList();

        foreach (var missingId in selectedIds.Where(id => retained.All(user => user.Id != id)))
        {
            var resolved = await TryGetUserAsync(missingId);
            if (resolved != null)
            {
                retained.Add(resolved);
            }
        }

        _selectedUsers = retained
            .OrderBy(user => user.DisplayName)
            .ThenBy(user => user.UserName)
            .ToList();
        _selectedUserIds = _selectedUsers.Select(user => user.Id).ToList();
        _displayItems = _selectedUsers.ToList();

        await UserIdsChanged.InvokeAsync(_selectedUserIds);
        await SelectedUsersChanged.InvokeAsync(_selectedUsers);
    }
}

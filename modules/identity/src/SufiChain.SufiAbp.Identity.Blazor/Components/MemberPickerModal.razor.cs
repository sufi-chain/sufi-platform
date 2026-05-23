using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.Identity.OrganizationUnits;
using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiAbp.UI.Blazor;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class MemberPickerModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadUsers = "load-users";
        public const string AddMembers = "add-members";
    }

    private IOrganizationUnitAppService OrganizationUnitAppService => LazyGetRequiredService(ref _organizationUnitAppService);
    private IOrganizationUnitAppService? _organizationUnitAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid OrganizationUnitId { get; set; }
    [Parameter] public EventCallback OnMembersAdded { get; set; }

    private List<OrganizationUnitMemberDto> _availableUsers = new();
    private HashSet<Guid> _selectedUserIds = new();
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;
    private string? _errorMessage;

    // Track previous open state to detect when modal opens
    private bool _wasOpen;
    private Guid _loadedForOuId;

    protected override async Task OnParametersSetAsync()
    {
        // Load data when modal transitions from closed to open, or when OrganizationUnitId changes while open
        var shouldLoad = Open && OrganizationUnitId != Guid.Empty && 
                        (!_wasOpen || _loadedForOuId != OrganizationUnitId);

        if (shouldLoad)
        {
            _selectedUserIds.Clear();
            _filter = null;
            _pageIndex = 0;
            _availableUsers.Clear();
            _errorMessage = null;
            _loadedForOuId = OrganizationUnitId;
            await LoadAvailableUsersAsync();
        }
        _wasOpen = Open;
    }

    private async Task LoadAvailableUsersAsync()
    {
        if (OrganizationUnitId == Guid.Empty) 
        {
            _errorMessage = "OrganizationUnitId is empty";
            return;
        }

        try
        {
            await ExecuteWithLoadingAsync(async () =>
            {
                var input = new GetOrganizationUnitMembersInput
                {
                    OrganizationUnitId = OrganizationUnitId,
                    Filter = _filter,
                    SkipCount = _pageIndex * _pageSize,
                    MaxResultCount = _pageSize
                };

                var result = await OrganizationUnitAppService.GetAvailableMembersAsync(input);
                _availableUsers = result.Items.ToList();
                _totalCount = result.TotalCount;
                _errorMessage = null;
            }, LoadingKeys.LoadUsers);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            Logger.LogError(ex, "Failed to load available members for OU {OrganizationUnitId}", OrganizationUnitId);
        }
    }

    private async Task OnFilterChangedAsync()
    {
        _pageIndex = 0;
        await LoadAvailableUsersAsync();
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadAvailableUsersAsync();
    }

    private void OnUserToggled(Guid userId, bool isSelected)
    {
        if (isSelected)
        {
            _selectedUserIds.Add(userId);
        }
        else
        {
            _selectedUserIds.Remove(userId);
        }
    }

    /// <summary>
    /// Called when SbDialog's open state changes (X button, backdrop click, escape key).
    /// </summary>
    private async Task OnDialogOpenChanged(bool isOpen)
    {
        if (!isOpen)
        {
            _wasOpen = false;
            await OpenChanged.InvokeAsync(false);
        }
    }

    private async Task Hide()
    {
        _wasOpen = false;
        await OpenChanged.InvokeAsync(false);
    }

    private Task AddMembersAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (_selectedUserIds.Count == 0) return;

        var input = new OrganizationUnitUserInput
        {
            OrganizationUnitId = OrganizationUnitId,
            UserIds = _selectedUserIds.ToList()
        };

        await OrganizationUnitAppService.AddMembersAsync(input);
        await OnMembersAdded.InvokeAsync();
        _wasOpen = false;
        await OpenChanged.InvokeAsync(false);
    }, LoadingKeys.AddMembers);
}

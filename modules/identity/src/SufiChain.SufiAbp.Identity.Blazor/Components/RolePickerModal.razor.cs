using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.Identity.OrganizationUnits;
using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class RolePickerModal : IdentityComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadRoles = "load-roles";
        public const string AddRoles = "add-roles";
    }

    private IOrganizationUnitAppService OrganizationUnitAppService => LazyGetRequiredService(ref _organizationUnitAppService);
    private IOrganizationUnitAppService? _organizationUnitAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid OrganizationUnitId { get; set; }
    [Parameter] public EventCallback OnRolesAdded { get; set; }

    private List<OrganizationUnitRoleDto> _availableRoles = new();
    private HashSet<Guid> _selectedRoleIds = new();
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
            _selectedRoleIds.Clear();
            _filter = null;
            _pageIndex = 0;
            _availableRoles.Clear();
            _errorMessage = null;
            _loadedForOuId = OrganizationUnitId;
            await LoadAvailableRolesAsync();
        }
        _wasOpen = Open;
    }

    private async Task LoadAvailableRolesAsync()
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
                var input = new GetOrganizationUnitRolesInput
                {
                    OrganizationUnitId = OrganizationUnitId,
                    Filter = _filter,
                    SkipCount = _pageIndex * _pageSize,
                    MaxResultCount = _pageSize
                };

                var result = await OrganizationUnitAppService.GetAvailableRolesAsync(input);
                _availableRoles = result.Items.ToList();
                _totalCount = result.TotalCount;
                _errorMessage = null;
            }, LoadingKeys.LoadRoles);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            Logger.LogError(ex, "Failed to load available roles for OU {OrganizationUnitId}", OrganizationUnitId);
        }
    }

    private async Task OnFilterChangedAsync()
    {
        _pageIndex = 0;
        await LoadAvailableRolesAsync();
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadAvailableRolesAsync();
    }

    private void OnRoleToggled(Guid roleId, bool isSelected)
    {
        if (isSelected)
        {
            _selectedRoleIds.Add(roleId);
        }
        else
        {
            _selectedRoleIds.Remove(roleId);
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

    private Task AddRolesAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (_selectedRoleIds.Count == 0) return;

        var input = new OrganizationUnitRoleInput
        {
            OrganizationUnitId = OrganizationUnitId,
            RoleIds = _selectedRoleIds.ToList()
        };

        await OrganizationUnitAppService.AddRolesAsync(input);
        await OnRolesAdded.InvokeAsync();
        _wasOpen = false;
        await OpenChanged.InvokeAsync(false);
    }, LoadingKeys.AddRoles);
}

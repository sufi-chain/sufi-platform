using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Identity.OrganizationUnits;
using SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Identity.Blazor.Components;

public partial class MemberPickerModal : IdentityComponentBase
{
    private static class LoadingKeys
    {
        public const string LoadMembers = "load-available-members";
        public const string AddMembers = "add-members";
    }

    private IOrganizationUnitAppService OrganizationUnitAppService => LazyGetRequiredService(ref _organizationUnitAppService);
    private IOrganizationUnitAppService? _organizationUnitAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public Guid OrganizationUnitId { get; set; }
    [Parameter] public EventCallback OnMembersAdded { get; set; }

    private SbDataGrid<OrganizationUnitMemberDto>? _gridRef;
    private HashSet<Guid> _selectedUserIds = new();
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;
    private string? _errorMessage;

    private bool _wasOpen;
    private Guid _loadedForOuId;

    protected override async Task OnParametersSetAsync()
    {
        var shouldLoad = Open && OrganizationUnitId != Guid.Empty &&
                         (!_wasOpen || _loadedForOuId != OrganizationUnitId);

        if (shouldLoad)
        {
            _selectedUserIds.Clear();
            _filter = null;
            _pageIndex = 0;
            _errorMessage = null;
            _loadedForOuId = OrganizationUnitId;
            await RefreshGridAsync();
        }

        _wasOpen = Open;
    }

    private async Task<SbDataResponse<OrganizationUnitMemberDto>> LoadAvailableMembersDataAsync(SbDataRequest request)
    {
        if (OrganizationUnitId == Guid.Empty)
        {
            _errorMessage = "OrganizationUnitId is empty";
            return new SbDataResponse<OrganizationUnitMemberDto>(Array.Empty<OrganizationUnitMemberDto>(), 0);
        }

        try
        {
            var result = await OrganizationUnitAppService.GetAvailableMembersAsync(new GetOrganizationUnitMembersInput
            {
                OrganizationUnitId = OrganizationUnitId,
                Filter = _filter,
                SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
                MaxResultCount = request.PageSize
            });

            _totalCount = result.TotalCount;
            _errorMessage = null;
            return new SbDataResponse<OrganizationUnitMemberDto>(result.Items, result.TotalCount);
        }
        catch (Exception ex)
        {
            _errorMessage = ex.Message;
            Logger.LogError(ex, "Failed to load available members for OU {OrganizationUnitId}", OrganizationUnitId);
            return new SbDataResponse<OrganizationUnitMemberDto>(Array.Empty<OrganizationUnitMemberDto>(), 0);
        }
    }

    private Task RefreshGridAsync()
    {
        return ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadMembers);
    }

    private async Task OnFilterChangedAsync()
    {
        _pageIndex = 0;
        await RefreshGridAsync();
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await RefreshGridAsync();
    }

    private void OnMemberToggled(Guid userId, bool isSelected)
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
        if (_selectedUserIds.Count == 0)
        {
            return;
        }

        await OrganizationUnitAppService.AddMembersAsync(new OrganizationUnitUserInput
        {
            OrganizationUnitId = OrganizationUnitId,
            UserIds = _selectedUserIds.ToList()
        });

        await OnMembersAdded.InvokeAsync();
        _wasOpen = false;
        await OpenChanged.InvokeAsync(false);
    }, LoadingKeys.AddMembers);
}

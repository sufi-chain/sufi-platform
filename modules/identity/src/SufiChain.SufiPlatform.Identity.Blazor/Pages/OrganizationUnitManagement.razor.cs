using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components.Overlays;
using SufiChain.SufiPlatform.Identity.OrganizationUnits;
using SufiChain.SufiPlatform.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Identity.Blazor.Pages;

public partial class OrganizationUnitManagement : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadTree = "load-tree";
        public const string LoadDetails = "load-details";
        public const string LoadMembers = "load-members";
        public const string LoadRoles = "load-roles";
        public const string Delete = "delete";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private IOrganizationUnitAppService OrganizationUnitAppService => LazyGetRequiredService(ref _organizationUnitAppService);
    private IOrganizationUnitAppService? _organizationUnitAppService;

    // Tree data
    private List<OrganizationUnitDto> _organizationUnits = new();
    private OrganizationUnitDto? _selectedUnit;
    private OrganizationUnitDto? _selectedUnitDetails;

    // Detail tabs
    private int _activeTab = 0;

    // Members
    private SbDataGrid<OrganizationUnitMemberDto>? _membersGridRef;
    private SbDataGrid<OrganizationUnitMemberDto>? _membersGridRefMobile;
    private string? _memberFilter;
    private int _memberPageIndex = 0;
    private int _memberPageSize = 10;
    private long _memberTotalCount;

    // Roles
    private SbDataGrid<OrganizationUnitRoleDto>? _rolesGridRef;
    private SbDataGrid<OrganizationUnitRoleDto>? _rolesGridRefMobile;
    private int _rolePageIndex = 0;
    private int _rolePageSize = 10;
    private long _roleTotalCount;

    // Modals
    private bool _showCreateModal;
    private Guid? _createParentId;
    private bool _showEditModal;
    private bool _showMemberPickerModal;
    private bool _showRolePickerModal;

    // Confirm dialog
    private SbConfirmDialog _confirmDialog = default!;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await LoadTreeAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["OrganizationUnits"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    #region Tree Operations

    private Task LoadTreeAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _organizationUnits = await OrganizationUnitAppService.GetTreeAsync();
        
        // Re-select if previously selected unit still exists
        if (_selectedUnit != null)
        {
            _selectedUnit = FindUnitInTree(_selectedUnit.Id, _organizationUnits);
            if (_selectedUnit != null)
            {
                await LoadUnitDetailsAsync();
            }
        }
    }, LoadingKeys.LoadTree);

    private OrganizationUnitDto? FindUnitInTree(Guid id, List<OrganizationUnitDto> units)
    {
        foreach (var unit in units)
        {
            if (unit.Id == id) return unit;
            var found = FindUnitInTree(id, unit.Children);
            if (found != null) return found;
        }
        return null;
    }

    private async Task OnUnitSelectedAsync(OrganizationUnitDto? unit)
    {
        _selectedUnit = unit;
        _activeTab = 0;
        
        if (unit != null)
        {
            await LoadUnitDetailsAsync();
        }
        else
        {
            _selectedUnitDetails = null;
            _memberTotalCount = 0;
            _roleTotalCount = 0;
        }
    }

    private async Task LoadUnitDetailsAsync()
    {
        if (_selectedUnit == null) return;

        await ExecuteWithLoadingAsync(async () =>
        {
            _selectedUnitDetails = await OrganizationUnitAppService.GetAsync(_selectedUnit.Id);
        }, LoadingKeys.LoadDetails);

        // Load members and roles in parallel
        await Task.WhenAll(RefreshMembersGridAsync(), RefreshRolesGridAsync());
    }

    #endregion

    #region CRUD Operations

    private void ShowCreateRootModal()
    {
        _createParentId = null;
        _showCreateModal = true;
    }

    private void ShowCreateChildModal()
    {
        _createParentId = _selectedUnit?.Id;
        _showCreateModal = true;
    }

    private void ShowEditModal()
    {
        _showEditModal = true;
    }

    private async Task OnUnitCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["OrganizationUnitCreatedSuccessfully"]);
        await LoadTreeAsync();
    }

    private async Task OnUnitUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["OrganizationUnitUpdatedSuccessfully"]);
        await LoadTreeAsync();
    }

    private async Task DeleteUnitAsync()
    {
        if (_selectedUnit == null) return;

        var hasChildren = _selectedUnit.Children.Count > 0;
        var message = hasChildren
            ? L["DeleteOrganizationUnitWithChildrenConfirmation", _selectedUnit.DisplayName]
            : L["DeleteOrganizationUnitConfirmation", _selectedUnit.DisplayName];

        if (!await Message.ConfirmAsync(message))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await OrganizationUnitAppService.DeleteAsync(_selectedUnit.Id);
            _selectedUnit = null;
            _selectedUnitDetails = null;
            await Notify.SuccessAsync(L["OrganizationUnitDeletedSuccessfully"]);
            await LoadTreeAsync();
        }, LoadingKeys.Delete);
    }

    #endregion

    #region Members

    private async Task<SbDataResponse<OrganizationUnitMemberDto>> LoadMembersDataAsync(SbDataRequest request)
    {
        if (_selectedUnit == null)
        {
            return new SbDataResponse<OrganizationUnitMemberDto>(Array.Empty<OrganizationUnitMemberDto>(), 0);
        }

        var result = await OrganizationUnitAppService.GetMembersAsync(new GetOrganizationUnitMembersInput
        {
            OrganizationUnitId = _selectedUnit.Id,
            Filter = _memberFilter,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        });

        _memberTotalCount = result.TotalCount;
        return new SbDataResponse<OrganizationUnitMemberDto>(result.Items, result.TotalCount);
    }

    private async Task RefreshMembersGridAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await (_membersGridRef?.RefreshDataAsync() ?? Task.CompletedTask);
            await (_membersGridRefMobile?.RefreshDataAsync() ?? Task.CompletedTask);
        }, LoadingKeys.LoadMembers);
    }

    private async Task OnMemberPageIndexChangedAsync(int pageIndex)
    {
        _memberPageIndex = pageIndex;
        await RefreshMembersGridAsync();
    }

    private void ShowMemberPickerModal()
    {
        _showMemberPickerModal = true;
    }

    private async Task OnMembersAddedAsync()
    {
        _showMemberPickerModal = false;
        await Notify.SuccessAsync(L["MembersAddedSuccessfully"]);
        await RefreshMembersGridAsync();
        await LoadUnitDetailsAsync();
    }

    private async Task RemoveMemberAsync(OrganizationUnitMemberDto member)
    {
        if (_selectedUnit == null) return;

        if (!await Message.ConfirmAsync(L["RemoveMemberConfirmation", member.UserName]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await OrganizationUnitAppService.RemoveMemberAsync(_selectedUnit.Id, member.UserId);
            await Notify.SuccessAsync(L["MemberRemovedSuccessfully"]);
            await RefreshMembersGridAsync();
            await LoadUnitDetailsAsync();
        }, LoadingKeys.LoadMembers);
    }

    #endregion

    #region Roles

    private async Task<SbDataResponse<OrganizationUnitRoleDto>> LoadRolesDataAsync(SbDataRequest request)
    {
        if (_selectedUnit == null)
        {
            return new SbDataResponse<OrganizationUnitRoleDto>(Array.Empty<OrganizationUnitRoleDto>(), 0);
        }

        var result = await OrganizationUnitAppService.GetRolesAsync(new GetOrganizationUnitRolesInput
        {
            OrganizationUnitId = _selectedUnit.Id,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        });

        _roleTotalCount = result.TotalCount;
        return new SbDataResponse<OrganizationUnitRoleDto>(result.Items, result.TotalCount);
    }

    private async Task RefreshRolesGridAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await (_rolesGridRef?.RefreshDataAsync() ?? Task.CompletedTask);
            await (_rolesGridRefMobile?.RefreshDataAsync() ?? Task.CompletedTask);
        }, LoadingKeys.LoadRoles);
    }

    private async Task OnRolePageIndexChangedAsync(int pageIndex)
    {
        _rolePageIndex = pageIndex;
        await RefreshRolesGridAsync();
    }

    private void ShowRolePickerModal()
    {
        _showRolePickerModal = true;
    }

    private async Task OnRolesAddedAsync()
    {
        _showRolePickerModal = false;
        await Notify.SuccessAsync(L["RolesAssignedSuccessfully"]);
        await RefreshRolesGridAsync();
        await LoadUnitDetailsAsync();
    }

    private async Task RemoveRoleAsync(OrganizationUnitRoleDto role)
    {
        if (_selectedUnit == null) return;

        if (!await Message.ConfirmAsync(L["RemoveRoleConfirmation", role.RoleName]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await OrganizationUnitAppService.RemoveRoleAsync(_selectedUnit.Id, role.RoleId);
            await Notify.SuccessAsync(L["RoleRemovedSuccessfully"]);
            await RefreshRolesGridAsync();
            await LoadUnitDetailsAsync();
        }, LoadingKeys.LoadRoles);
    }

    #endregion
}

using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Blazor.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Blazor.Pages;

public partial class RoleManagement : IdentityComponentBase
{

    private const string PermissionProviderName = "R"; // Role provider

    private static class LoadingKeys
    {
        public const string LoadRoles = "load-roles";
        public const string DeleteRole = "delete-role";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private IIdentityRoleAppService RoleAppService => LazyGetRequiredService(ref _roleAppService);
    private IIdentityRoleAppService? _roleAppService;

    private List<IdentityRoleDto> _roles = new();
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private IdentityRoleDto? _selectedRole;

    // Permission management
    private PermissionManagementModal? _permissionManagementModal;
    private bool _hasManagePermissionsPermission;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            // Check if user has permission to manage permissions
            _hasManagePermissionsPermission = await IsGrantedAsync(IdentityPermissions.Roles.ManagePermissions);

            await LoadRolesAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Roles"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private Task LoadRolesAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var input = new GetIdentityRolesInput
        {
            Filter = _filter,
            SkipCount = _pageIndex * _pageSize,
            MaxResultCount = _pageSize
        };

        var result = await RoleAppService.GetListAsync(input);
        _roles = result.Items.ToList();
        _totalCount = result.TotalCount;
    }, LoadingKeys.LoadRoles);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadRolesAsync();
    }

    private void ShowCreateModal()
    {
        _showCreateModal = true;
    }

    private void ShowEditModal(IdentityRoleDto role)
    {
        _selectedRole = role;
        _showEditModal = true;
    }

    private async Task OnRoleCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["RoleCreatedSuccessfully"]);
        await LoadRolesAsync();
    }

    private async Task OnRoleUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["RoleUpdatedSuccessfully"]);
        await LoadRolesAsync();
    }

    private async Task DeleteRoleAsync(IdentityRoleDto role)
    {
        if (role.IsStatic)
            return;

        if (!await Message.ConfirmAsync(L["DeleteRoleConfirmation", role.Name!]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await RoleAppService.DeleteAsync(role.Id);
            await Notify.SuccessAsync(L["RoleDeletedSuccessfully"]);
            await LoadRolesAsync();
        }, LoadingKeys.DeleteRole);
    }

    private async Task ShowPermissionModalAsync(IdentityRoleDto role)
    {
        if (_permissionManagementModal != null)
        {
            await _permissionManagementModal.OpenAsync(
                PermissionProviderName,
                role.Name!, // Role provider uses role name as key
                role.Name
            );
        }
    }
}

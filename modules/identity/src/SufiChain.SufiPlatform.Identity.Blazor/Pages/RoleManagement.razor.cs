using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Identity.Blazor.Components;
using SufiChain.SufiPlatform.UI.Layout;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

namespace SufiChain.SufiPlatform.Identity.Blazor.Pages;

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

    private SbDataGrid<IdentityRoleDto>? _gridRef;
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private IdentityRoleDto? _selectedRole;

    // Permission management
    private PermissionsModal? _permissionManagementModal;
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

            await RefreshGridAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Roles"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private async Task<SbDataResponse<IdentityRoleDto>> LoadRolesDataAsync(SbDataRequest request)
    {
        var result = await RoleAppService.GetListAsync(new GetIdentityRolesInput
        {
            Filter = _filter,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        });

        _totalCount = result.TotalCount;
        return new SbDataResponse<IdentityRoleDto>(result.Items, result.TotalCount);
    }

    private Task RefreshGridAsync()
    {
        return ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadRoles);
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await RefreshGridAsync();
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
        await RefreshGridAsync();
    }

    private async Task OnRoleUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["RoleUpdatedSuccessfully"]);
        await RefreshGridAsync();
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
            await RefreshGridAsync();
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

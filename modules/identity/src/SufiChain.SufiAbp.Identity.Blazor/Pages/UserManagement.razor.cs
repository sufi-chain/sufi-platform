using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Blazor.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Blazor.Pages;

public partial class UserManagement : IdentityComponentBase
{

    private const string PermissionProviderName = "U"; // User provider

    private static class LoadingKeys
    {
        public const string LoadUsers = "load-users";
        public const string LoadRoles = "load-roles";
        public const string DeleteUser = "delete-user";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private IIdentityUserAppService UserAppService => LazyGetRequiredService(ref _userAppService);
    private IIdentityUserAppService? _userAppService;

    private IIdentityRoleAppService RoleAppService => LazyGetRequiredService(ref _roleAppService);
    private IIdentityRoleAppService? _roleAppService;

    private List<IdentityUserDto> _users = new();
    private List<IdentityRoleDto> _roles = new();
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private IdentityUserDto? _selectedUser;

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
            _hasManagePermissionsPermission = await IsGrantedAsync(IdentityPermissions.Users.ManagePermissions);

            await ExecuteWithLoadingAsync(async () =>
            {
                var result = await RoleAppService.GetAllListAsync();
                _roles = result.Items.ToList();
            }, LoadingKeys.LoadRoles);

            await LoadUsersAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Users"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private Task LoadUsersAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var input = new GetIdentityUsersInput
        {
            Filter = _filter,
            SkipCount = _pageIndex * _pageSize,
            MaxResultCount = _pageSize
        };

        var result = await UserAppService.GetListAsync(input);
        _users = result.Items.ToList();
        _totalCount = result.TotalCount;
    }, LoadingKeys.LoadUsers);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadUsersAsync();
    }

    private void ShowCreateModal()
    {
        _showCreateModal = true;
    }

    private void ShowEditModal(IdentityUserDto user)
    {
        _selectedUser = user;
        _showEditModal = true;
    }

    private async Task OnUserCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["UserCreatedSuccessfully"]);
        await LoadUsersAsync();
    }

    private async Task OnUserUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["UserUpdatedSuccessfully"]);
        await LoadUsersAsync();
    }

    private async Task DeleteUserAsync(IdentityUserDto user)
    {
        if (!await Message.ConfirmAsync(L["DeleteUserConfirmation", user.UserName!]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await UserAppService.DeleteAsync(user.Id);
            await Notify.SuccessAsync(L["UserDeletedSuccessfully"]);
            await LoadUsersAsync();
        }, LoadingKeys.DeleteUser);
    }

    private async Task ShowPermissionModalAsync(IdentityUserDto user)
    {
        if (_permissionManagementModal != null)
        {
            await _permissionManagementModal.OpenAsync(
                PermissionProviderName,
                user.Id.ToString(),
                user.UserName
            );
        }
    }
}

using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Identity.Blazor.Components;

public partial class UserEditor : IdentityComponentBase
{
    private const string UsersRoute = "/panel/admin/identity/users";

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    private IIdentityUserAppService UserAppService => LazyGetRequiredService(ref _userAppService);
    private IIdentityUserAppService? _userAppService;

    private IIdentityRoleAppService RoleAppService => LazyGetRequiredService(ref _roleAppService);
    private IIdentityRoleAppService? _roleAppService;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter]
    public Guid? UserId { get; set; }

    private readonly IdentityUserCreateDto _createModel = new()
    {
        IsActive = true,
        LockoutEnabled = true
    };

    private IdentityUserUpdateDto _updateModel = new();
    private List<IdentityRoleDto> _roles = new();
    private HashSet<string> _selectedRoles = new();
    private int _activeTab;
    private bool _ready;
    private bool _loadFailed;
    private Guid? _loadedUserId;

    private bool IsEdit => UserId.HasValue;

    private IdentityUserCreateOrUpdateDtoBase Editor => IsEdit ? _updateModel : _createModel;

    private object FormModel => IsEdit ? _updateModel : _createModel;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender || _loadedUserId != UserId)
        {
            _loadedUserId = UserId;
            await LoadAsync();
        }
    }

    private async Task LoadAsync()
    {
        _ready = false;
        _loadFailed = false;
        _activeTab = 0;
        await ExecuteWithLoadingAsync(async () =>
        {
            var roles = await RoleAppService.GetAllListAsync();
            _roles = roles.Items.ToList();

            if (UserId is Guid userId)
            {
                var user = await UserAppService.GetAsync(userId);
                _updateModel = new IdentityUserUpdateDto
                {
                    UserName = user.UserName,
                    Name = user.Name,
                    Surname = user.Surname,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    IsActive = user.IsActive,
                    LockoutEnabled = user.LockoutEnabled,
                    ConcurrencyStamp = user.ConcurrencyStamp
                };

                var userRoles = await UserAppService.GetRolesAsync(userId);
                _selectedRoles = userRoles.Items.Select(r => r.Name).ToHashSet();
            }
            else
            {
                _selectedRoles = new HashSet<string>();
            }

            _ready = true;
        }, LoadingKeys.Load);

        if (!_ready && !IsDisposed)
        {
            _loadFailed = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void OnRoleToggled(string roleName, bool isSelected)
    {
        if (isSelected)
        {
            _selectedRoles.Add(roleName);
        }
        else
        {
            _selectedRoles.Remove(roleName);
        }
    }

    private void Cancel()
    {
        NavigationManager.NavigateTo(UsersRoute);
    }

    private Task SaveAsync() => ExecuteWithLoadingAsync(async () =>
    {
        if (!_ready)
        {
            return;
        }

        if (UserId is Guid userId)
        {
            _updateModel.RoleNames = _selectedRoles.ToArray();
            await UserAppService.UpdateAsync(userId, _updateModel);
            await Notify.SuccessAsync(L["UserUpdatedSuccessfully"]);
        }
        else
        {
            _createModel.RoleNames = _selectedRoles.ToArray();
            await UserAppService.CreateAsync(_createModel);
            await Notify.SuccessAsync(L["UserCreatedSuccessfully"]);
        }

        NavigationManager.NavigateTo(UsersRoute);
    }, LoadingKeys.Save);
}

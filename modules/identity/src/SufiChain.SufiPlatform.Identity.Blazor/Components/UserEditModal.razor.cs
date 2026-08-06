using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Identity.Blazor.Components;

public partial class UserEditModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string LoadRoles = "load-roles";
        public const string Save = "save";
    }

    private IIdentityUserAppService UserAppService => LazyGetRequiredService(ref _userAppService);
    private IIdentityUserAppService? _userAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public IdentityUserDto? User { get; set; }
    [Parameter] public EventCallback OnUserUpdated { get; set; }
    [Parameter] public List<IdentityRoleDto> Roles { get; set; } = new();

    private IdentityUserUpdateDto _model = new();
    private HashSet<string> _selectedRoles = new();
    private int _activeTab = 0;
    private Guid _userId;

    protected override async Task OnParametersSetAsync()
    {
        if (Open && User != null && User.Id != _userId)
        {
            _userId = User.Id;
            _model = new IdentityUserUpdateDto
            {
                UserName = User.UserName,
                Name = User.Name,
                Surname = User.Surname,
                Email = User.Email,
                PhoneNumber = User.PhoneNumber,
                IsActive = User.IsActive,
                LockoutEnabled = User.LockoutEnabled,
                ConcurrencyStamp = User.ConcurrencyStamp
            };

            await ExecuteWithLoadingAsync(async () =>
            {
                var userRoles = await UserAppService.GetRolesAsync(_userId);
                _selectedRoles = userRoles.Items.Select(r => r.Name).ToHashSet();
            }, LoadingKeys.LoadRoles);

            _activeTab = 0;
        }
    }

    private Task Hide()
    {
        return SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        await OpenChanged.InvokeAsync(open);
    }

    private void OnRoleToggled(string roleName, bool isSelected)
    {
        if (isSelected)
            _selectedRoles.Add(roleName);
        else
            _selectedRoles.Remove(roleName);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _model.RoleNames = _selectedRoles.ToArray();
        await UserAppService.UpdateAsync(_userId, _model);
        await OnUserUpdated.InvokeAsync();
    }, LoadingKeys.Save);
}

using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class UserCreateModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IIdentityUserAppService UserAppService => LazyGetRequiredService(ref _userAppService);
    private IIdentityUserAppService? _userAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnUserCreated { get; set; }
    [Parameter] public List<IdentityRoleDto> Roles { get; set; } = new();

    private IdentityUserCreateDto _model = new();
    private HashSet<string> _selectedRoles = new();
    private int _activeTab = 0;

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new IdentityUserCreateDto
            {
                IsActive = true,
                LockoutEnabled = true
            };
            _selectedRoles.Clear();
            _activeTab = 0;
        }
    }

    private void Hide()
    {
        OpenChanged.InvokeAsync(false);
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
        await UserAppService.CreateAsync(_model);
        await OnUserCreated.InvokeAsync();
    }, LoadingKeys.Create);
}

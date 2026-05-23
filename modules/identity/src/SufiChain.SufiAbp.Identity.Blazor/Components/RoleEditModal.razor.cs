using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class RoleEditModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private IIdentityRoleAppService RoleAppService => LazyGetRequiredService(ref _roleAppService);
    private IIdentityRoleAppService? _roleAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public IdentityRoleDto? Role { get; set; }
    [Parameter] public EventCallback OnRoleUpdated { get; set; }

    private IdentityRoleUpdateDto _model = new();
    private Guid _roleId;

    protected override void OnParametersSet()
    {
        if (Open && Role != null && Role.Id != _roleId)
        {
            _roleId = Role.Id;
            _model = new IdentityRoleUpdateDto
            {
                Name = Role.Name,
                IsDefault = Role.IsDefault,
                IsPublic = Role.IsPublic,
                ConcurrencyStamp = Role.ConcurrencyStamp
            };
        }
    }

    private void Hide()
    {
        OpenChanged.InvokeAsync(false);
    }

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await RoleAppService.UpdateAsync(_roleId, _model);
        await OnRoleUpdated.InvokeAsync();
    }, LoadingKeys.Save);
}

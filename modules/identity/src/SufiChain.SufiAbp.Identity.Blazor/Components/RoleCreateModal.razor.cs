using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Identity.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.Identity;

namespace SufiChain.SufiAbp.Identity.Blazor.Components;

public partial class RoleCreateModal : IdentityComponentBase
{

    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private IIdentityRoleAppService RoleAppService => LazyGetRequiredService(ref _roleAppService);
    private IIdentityRoleAppService? _roleAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnRoleCreated { get; set; }

    private IdentityRoleCreateDto _model = new();

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new IdentityRoleCreateDto();
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

    private Task OnValidSubmitAsync() => ExecuteWithLoadingAsync(async () =>
    {
        await RoleAppService.CreateAsync(_model);
        await OnRoleCreated.InvokeAsync();
    }, LoadingKeys.Create);
}

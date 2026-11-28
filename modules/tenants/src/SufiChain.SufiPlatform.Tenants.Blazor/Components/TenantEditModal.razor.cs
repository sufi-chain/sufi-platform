using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants.Blazor.Components;

public partial class TenantEditModal : TenantsComponentBase
{

    private static class LoadingKeys
    {
        public const string Save = "save";
    }

    private ITenantAppService TenantAppService => LazyGetRequiredService(ref _tenantAppService);
    private ITenantAppService? _tenantAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public TenantDto? Tenant { get; set; }
    [Parameter] public EventCallback OnTenantUpdated { get; set; }

    private TenantUpdateDto _model = new();
    private Guid _tenantId;
    private bool _active;

    protected override void OnParametersSet()
    {
        if (Open && Tenant != null && Tenant.Id != _tenantId)
        {
            _tenantId = Tenant.Id;
            _model = new TenantUpdateDto
            {
                Name = Tenant.Name,
                ConcurrencyStamp = Tenant.ConcurrencyStamp
            };
            _active = Tenant.IsActive;
        }
    }

    private async Task OnActiveChangedAsync(bool active)
    {
        if (active == _active || Tenant == null)
        {
            return;
        }

        var confirmed = await Message.ConfirmAsync(
            active
                ? L["ActivateTenantConfirmation", Tenant.Name]
                : L["DeactivateTenantConfirmation", Tenant.Name]);
        if (!confirmed)
        {
            return;
        }

        if (active)
        {
            await TenantAppService.ActivateAsync(_tenantId);
        }
        else
        {
            await TenantAppService.DeactivateAsync(_tenantId);
        }

        _active = active;
        await OnTenantUpdated.InvokeAsync();
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
        await TenantAppService.UpdateAsync(_tenantId, _model);
        await OnTenantUpdated.InvokeAsync();
        await Hide();
    }, LoadingKeys.Save);
}

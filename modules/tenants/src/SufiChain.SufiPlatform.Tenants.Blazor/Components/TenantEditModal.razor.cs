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
        await TenantAppService.UpdateAsync(_tenantId, _model);
        await OnTenantUpdated.InvokeAsync();
        Hide();
    }, LoadingKeys.Save);
}

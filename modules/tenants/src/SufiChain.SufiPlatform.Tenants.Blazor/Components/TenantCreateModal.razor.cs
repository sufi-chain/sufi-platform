using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Components.Forms;
using SufiChain.SufiPlatform.Tenants.Localization;
using SufiChain.SufiPlatform.UI.Blazor;
using SufiChain.SufiPlatform.Tenants;

namespace SufiChain.SufiPlatform.Tenants.Blazor.Components;

public partial class TenantCreateModal : TenantsComponentBase
{

    private static class LoadingKeys
    {
        public const string Create = "create";
    }

    private ITenantAppService TenantAppService => LazyGetRequiredService(ref _tenantAppService);
    private ITenantAppService? _tenantAppService;

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public EventCallback OnTenantCreated { get; set; }

    private SbForm? _formRef;
    private TenantCreateDto _model = new();

    protected override void OnParametersSet()
    {
        if (Open)
        {
            _model = new TenantCreateDto();
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
        await TenantAppService.CreateAsync(_model);
        await OnTenantCreated.InvokeAsync();
        Hide();
    }, LoadingKeys.Create);
}

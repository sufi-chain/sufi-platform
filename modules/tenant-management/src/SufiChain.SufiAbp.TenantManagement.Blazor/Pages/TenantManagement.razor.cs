using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.FeatureManagement.Blazor.Components;
using SufiChain.SufiAbp.SettingManagement.Blazor.Components;
using SufiChain.SufiAbp.TenantManagement.Localization;
using SufiChain.SufiAbp.UI.Blazor;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.TenantManagement;

namespace SufiChain.SufiAbp.TenantManagement.Blazor.Pages;

public partial class TenantManagement : TenantManagementComponentBase
{
    /// <summary>
    /// Provider name for tenant features ("T" = TenantFeatureValueProvider.ProviderName)
    /// </summary>
    private const string TenantFeatureProviderName = "T";

    private static class LoadingKeys
    {
        public const string LoadTenants = "load-tenants";
        public const string DeleteTenant = "delete-tenant";
    }

    [Inject] protected IPageLayout PageLayout { get; set; } = default!;

    private ITenantAppService TenantAppService => LazyGetRequiredService(ref _tenantAppService);
    private ITenantAppService? _tenantAppService;

    private List<TenantDto> _tenants = new();
    private string? _filter;
    private int _pageIndex = 0;
    private int _pageSize = 10;
    private long _totalCount;

    private bool _showCreateModal;
    private bool _showEditModal;
    private TenantDto? _selectedTenant;

    private FeatureManagementModal _featureManagementModal = default!;
    private SettingManagementModal _settingManagementModal = default!;

    protected override void OnInitialized()
    {
        SetupPageLayout();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        
        if (firstRender)
        {
            await LoadTenantsAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Tenants"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private Task LoadTenantsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        var input = new GetTenantsInput
        {
            Filter = _filter,
            SkipCount = _pageIndex * _pageSize,
            MaxResultCount = _pageSize
        };

        var result = await TenantAppService.GetListAsync(input);
        _tenants = result.Items.ToList();
        _totalCount = result.TotalCount;
    }, LoadingKeys.LoadTenants);

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadTenantsAsync();
    }

    private void ShowCreateModal()
    {
        _showCreateModal = true;
    }

    private void ShowEditModal(TenantDto tenant)
    {
        _selectedTenant = tenant;
        _showEditModal = true;
    }

    private async Task OnTenantCreatedAsync()
    {
        _showCreateModal = false;
        await Notify.SuccessAsync(L["TenantCreatedSuccessfully"]);
        await LoadTenantsAsync();
    }

    private async Task OnTenantUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["TenantUpdatedSuccessfully"]);
        await LoadTenantsAsync();
    }

    private async Task DeleteTenantAsync(TenantDto tenant)
    {
        if (!await Message.ConfirmAsync(L["DeleteTenantConfirmation", tenant.Name!]))
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await TenantAppService.DeleteAsync(tenant.Id);
            await Notify.SuccessAsync(L["TenantDeletedSuccessfully"]);
            await LoadTenantsAsync();
        }, LoadingKeys.DeleteTenant);
    }

    /// <summary>
    /// Opens the feature management modal for host-level features.
    /// </summary>
    private async Task ShowHostFeaturesModal()
    {
        await _featureManagementModal.OpenAsync(
            TenantFeatureProviderName,
            null, // null key = host features
            L["HostFeatures"]
        );
    }

    /// <summary>
    /// Opens the feature management modal for a specific tenant's features.
    /// </summary>
    private async Task ShowTenantFeaturesModal(TenantDto tenant)
    {
        await _featureManagementModal.OpenAsync(
            TenantFeatureProviderName,
            tenant.Id.ToString(),
            tenant.Name
        );
    }


}

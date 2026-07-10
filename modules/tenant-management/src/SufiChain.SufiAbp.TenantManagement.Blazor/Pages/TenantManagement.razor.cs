using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.FeatureManagement.Blazor.Components;
using SufiChain.SufiAbp.SettingManagement.Blazor.Components;
using SufiChain.SufiAbp.UI.Layout;
using SufiChain.SufiAbp.TenantManagement;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiBlazor.Contracts.Data;

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

    private SbDataGrid<TenantDto>? _gridRef;
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
            await RefreshGridAsync();
        }
    }

    private void SetupPageLayout()
    {
        PageLayout.Title = L["Tenants"];
        // Breadcrumbs are auto-generated from menu hierarchy by the layout
    }

    private async Task<SbDataResponse<TenantDto>> LoadTenantsDataAsync(SbDataRequest request)
    {
        var result = await TenantAppService.GetListAsync(new GetTenantsInput
        {
            Filter = _filter,
            SkipCount = Math.Max(0, request.PageIndex * request.PageSize),
            MaxResultCount = request.PageSize
        });

        _totalCount = result.TotalCount;
        return new SbDataResponse<TenantDto>(result.Items, result.TotalCount);
    }

    private Task RefreshGridAsync()
    {
        return ExecuteWithLoadingAsync(
            () => _gridRef?.RefreshDataAsync() ?? Task.CompletedTask,
            LoadingKeys.LoadTenants);
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await RefreshGridAsync();
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
        await RefreshGridAsync();
    }

    private async Task OnTenantUpdatedAsync()
    {
        _showEditModal = false;
        await Notify.SuccessAsync(L["TenantUpdatedSuccessfully"]);
        await RefreshGridAsync();
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
            await RefreshGridAsync();
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

using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.UI.MultiTenancy;

namespace SufiChain.SufiAbp.UI.Blazor.Components.TenantSelector;

public partial class SufiAbpTenantSelector
{
    private static class LoadingKeys
    {
        public const string LoadTenants = "load-tenants";
        public const string Switch = "switch";
    }

    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }
    [Parameter] public string Mode { get; set; } = "InputName";
    [Parameter] public TenantSelectorRenderMode RenderMode { get; set; } = TenantSelectorRenderMode.Dialog;

    private string _tenantName = string.Empty;
    private List<TenantLookupItemDto> _tenantItems = new();
    private TenantLookupItemDto? _selectedTenantForSearch;
    private int _pageIndex;
    private const int PageSize = 10;
    private long _totalCount;
    private bool _wasOpen;

    private bool IsListMode => Mode is "SelectFromList" or "Search";

    protected override async Task OnParametersSetAsync()
    {
        if (!IsListMode)
        {
            return;
        }

        // Load tenants when dialog opens (SelectFromList only; Search uses SearchFunc on demand)
        if (Open && !_wasOpen && Mode == "SelectFromList")
        {
            _pageIndex = 0;
            _tenantItems.Clear();
            await LoadTenantsAsync();
        }

        _wasOpen = Open;
    }

    private async Task LoadTenantsAsync()
    {
        var lookupService = ScopedServices.GetService<ITenantLookupService>();
        if (lookupService == null)
        {
            _tenantItems = new List<TenantLookupItemDto>();
            _totalCount = 0;
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var result = await lookupService.GetListAsync(null, _pageIndex * PageSize, PageSize);
            _tenantItems = result.Items ?? new List<TenantLookupItemDto>();
            _totalCount = result.TotalCount;
        }, LoadingKeys.LoadTenants);
    }

    private async Task<IEnumerable<TenantLookupItemDto>> SearchTenantsAsync(string searchText)
    {
        var lookupService = ScopedServices.GetService<ITenantLookupService>();
        if (lookupService == null)
        {
            return Array.Empty<TenantLookupItemDto>();
        }

        var result = await lookupService.GetListAsync(
            string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim(),
            0,
            50);
        return result.Items ?? new List<TenantLookupItemDto>();
    }

    private async Task OnPageIndexChangedAsync(int pageIndex)
    {
        _pageIndex = pageIndex;
        await LoadTenantsAsync();
    }

    private async Task OnTenantSelectedFromSearchAsync()
    {
        if (_selectedTenantForSearch == null || !IsInteractive)
        {
            return;
        }

        Logger.LogInformation("Switching to tenant {TenantId} ({Name})", _selectedTenantForSearch.Id, _selectedTenantForSearch.Name);
        await ExecuteWithLoadingAsync(
            () => TenantSwitchService.SwitchTenantAsync(_selectedTenantForSearch.Id, _selectedTenantForSearch.Name),
            LoadingKeys.Switch,
            LoadingBehavior.None);
        _selectedTenantForSearch = null;
        await Hide();
    }

    private async Task SwitchToTenantAsync(TenantLookupItemDto tenant)
    {
        if (!IsInteractive)
        {
            Logger.LogWarning("SwitchToTenantAsync: component is not interactive (prerendering). Tenant switch deferred.");
            return;
        }

        Logger.LogInformation("Switching to tenant {TenantId} ({Name})", tenant.Id, tenant.Name);
        await ExecuteWithLoadingAsync(
            () => TenantSwitchService.SwitchTenantAsync(tenant.Id, tenant.Name),
            LoadingKeys.Switch,
            LoadingBehavior.None);
    }

    private async Task SwitchToHostAsync()
    {
        if (!IsInteractive)
        {
            Logger.LogWarning("SwitchToHostAsync: component is not interactive (prerendering). Tenant switch deferred.");
            return;
        }

        Logger.LogInformation("Switching to host");
        await ExecuteWithLoadingAsync(
            () => TenantSwitchService.SwitchTenantAsync(null, null),
            LoadingKeys.Switch,
            LoadingBehavior.None);
    }

    private async Task Hide()
    {
        _selectedTenantForSearch = null;
        await SetOpenAsync(false);
    }

    private async Task SetOpenAsync(bool open)
    {
        Open = open;
        _wasOpen = open;
        await OpenChanged.InvokeAsync(open);
    }

    private async Task SwitchByHostNameAsync()
    {
        var tenantName = string.IsNullOrWhiteSpace(_tenantName) ? null : _tenantName.Trim();
        Logger.LogInformation("SwitchByHostNameAsync: switching to {Target}", tenantName is null ? "host" : $"tenant '{tenantName}'");
        await ExecuteWithLoadingAsync(
            () => TenantSwitchService.SwitchTenantAsync(null, tenantName),
            LoadingKeys.Switch,
            LoadingBehavior.None);
        _tenantName = string.Empty;
        await Hide();
    }
}

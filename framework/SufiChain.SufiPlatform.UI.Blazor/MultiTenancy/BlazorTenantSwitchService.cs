using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.MultiTenancy;

namespace SufiChain.SufiPlatform.UI.Blazor.MultiTenancy;

/// <summary>
/// Blazor implementation of ITenantSwitchService. Sets the tenant cookie via HTTP redirect
/// (/Account/SwitchTenant) so it works during prerendering and in all render modes.
/// Avoids JavaScript interop which is not available during static prerendering.
/// </summary>
public class BlazorTenantSwitchService : ITenantSwitchService
{
    private readonly NavigationManager _navigationManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly TenantSwitchOptions _options;
    private readonly ILogger<BlazorTenantSwitchService> _logger;

    public BlazorTenantSwitchService(
        NavigationManager navigationManager,
        ICurrentTenant currentTenant,
        IOptions<TenantSwitchOptions> options,
        ILogger<BlazorTenantSwitchService> logger)
    {
        _navigationManager = navigationManager;
        _currentTenant = currentTenant;
        _options = options.Value;
        _logger = logger;
    }

    public Guid? CurrentTenantId => _currentTenant.Id;
    public string? CurrentTenantName => _currentTenant.Name;

    public Task SwitchTenantAsync(Guid? tenantId, string? tenantName = null)
    {
        _logger.LogInformation("SwitchTenantAsync called – tenantId: {TenantId}, tenantName: {TenantName}",
            tenantId, tenantName);

        var cookieName = _options.TenantCookieName;
        if (string.IsNullOrEmpty(cookieName))
        {
            _logger.LogWarning("TenantCookieName is not configured; tenant switch aborted.");
            return Task.CompletedTask;
        }

        // Build the return URL. NavigationManager may not be initialized if this service
        // was resolved from an OwningComponentBase child scope instead of the circuit scope.
        // In that case the child-scope NavigationManager hasn't been attached to the circuit.
        // Callers should inject this service via @inject (circuit scope), not LazyGetRequiredService.
        string returnUrl;
        string baseUri;
        try
        {
            // NavigationManager.Uri returns the full URL (e.g. https://host/Account/Login).
            // The /Account/SwitchTenant middleware validates that returnUrl starts with "/"
            // (local path), so we must convert to a relative path+query.
            var fullUri = new Uri(_navigationManager.Uri);
            returnUrl = fullUri.PathAndQuery;
            baseUri = _navigationManager.BaseUri.TrimEnd('/');
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "NavigationManager is not initialized. This usually means the service was resolved " +
                "from an OwningComponentBase child scope instead of the circuit scope. " +
                "Ensure ITenantSwitchService is injected via @inject, not via LazyGetRequiredService/ScopedServices. " +
                "Tenant switch aborted.");
            return Task.CompletedTask;
        }

        var query = new List<string>();
        if (tenantId.HasValue)
        {
            query.Add($"tenantId={tenantId.Value}");
        }
        if (!string.IsNullOrEmpty(tenantName))
        {
            query.Add($"tenantName={Uri.EscapeDataString(tenantName)}");
        }
        query.Add($"returnUrl={Uri.EscapeDataString(returnUrl)}");

        var switchUrl = $"{baseUri}/Account/SwitchTenant?{string.Join("&", query)}";
        _logger.LogInformation("Navigating to tenant switch URL: {SwitchUrl}", switchUrl);
        _navigationManager.NavigateTo(switchUrl, forceLoad: true);

        return Task.CompletedTask;
    }
}

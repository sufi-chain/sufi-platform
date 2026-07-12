namespace SufiChain.SufiPlatform.UI.MultiTenancy;

/// <summary>
/// Service for switching tenants in the UI.
/// </summary>
public interface ITenantSwitchService
{
    /// <summary>
    /// Switches to a different tenant.
    /// </summary>
    /// <param name="tenantId">The tenant ID to switch to. Null to switch to host.</param>
    /// <param name="tenantName">Optional tenant name for display purposes.</param>
    Task SwitchTenantAsync(Guid? tenantId, string? tenantName = null);

    /// <summary>
    /// Gets the current tenant ID.
    /// </summary>
    Guid? CurrentTenantId { get; }

    /// <summary>
    /// Gets the current tenant name.
    /// </summary>
    string? CurrentTenantName { get; }
}

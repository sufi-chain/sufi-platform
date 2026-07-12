namespace SufiChain.SufiPlatform.UI.MultiTenancy;

/// <summary>
/// Service for looking up tenants (list/search) for the tenant selector.
/// Default implementation returns empty list. Tenant-management module provides
/// a real implementation when loaded.
/// </summary>
public interface ITenantLookupService
{
    /// <summary>
    /// Gets a paged list of tenants for lookup.
    /// Used by SelectFromList and Search modes of the tenant selector.
    /// </summary>
    /// <param name="filter">Optional filter for tenant name.</param>
    /// <param name="skipCount">Number of items to skip (pagination).</param>
    /// <param name="maxResultCount">Maximum number of items to return.</param>
    Task<TenantLookupResult> GetListAsync(string? filter, int skipCount, int maxResultCount);
}

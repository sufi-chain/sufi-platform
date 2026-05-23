namespace SufiChain.SufiAbp.TenantManagement.Tenants;

/// <summary>
/// Minimal DTO for tenant lookup (e.g. tenant selector on login page).
/// Only Id and Name are exposed for anonymous/public listing.
/// </summary>
public class TenantLookupItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

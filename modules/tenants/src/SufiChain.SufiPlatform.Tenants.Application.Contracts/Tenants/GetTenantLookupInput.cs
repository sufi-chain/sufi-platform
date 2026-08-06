using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Tenants.Tenants;

/// <summary>
/// Input for tenant lookup (anonymous list/search).
/// </summary>
public class GetTenantLookupInput : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// Optional filter for tenant name (case-insensitive).
    /// </summary>
    public string? Filter { get; set; }
}

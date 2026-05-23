namespace SufiChain.SufiAbp.UI.MultiTenancy;

/// <summary>
/// Result of tenant lookup (paged list).
/// Used by ITenantLookupService to keep UI.Abstractions independent from backend DTO packages.
/// </summary>
public class TenantLookupResult
{
    public long TotalCount { get; set; }

    public List<TenantLookupItemDto> Items { get; set; } = new();
}

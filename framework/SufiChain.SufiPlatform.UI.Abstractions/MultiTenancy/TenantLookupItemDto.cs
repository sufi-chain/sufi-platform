namespace SufiChain.SufiPlatform.UI.MultiTenancy;

/// <summary>
/// Minimal DTO for tenant lookup (tenant selector list/search).
/// Used by ITenantLookupService when tenant-management module provides the implementation.
/// </summary>
public class TenantLookupItemDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
}

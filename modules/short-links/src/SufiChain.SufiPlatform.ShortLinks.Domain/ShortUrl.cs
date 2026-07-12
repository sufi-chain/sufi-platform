using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrl : AuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    public string ShortCode { get; set; } = string.Empty;
    
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    public int ClickCount { get; set; }
    
    public DateTime? LastAccessedAt { get; set; }
    
    public bool IsActive { get; set; }
    
    public string CreatedByModule { get; set; } = string.Empty;
    
    public string? Description { get; set; }

    protected ShortUrl()
    {
    }
    
    public ShortUrl(
        Guid id,
        string shortCode,
        string destinationUrl,
        string createdByModule,
        DateTime? expiresAt = null,
        string? description = null)
        : base(id)
    {
        ShortCode = shortCode;
        DestinationUrl = destinationUrl;
        CreatedByModule = createdByModule;
        ExpiresAt = expiresAt;
        Description = description;
        IsActive = true;
        ClickCount = 0;
    }
}

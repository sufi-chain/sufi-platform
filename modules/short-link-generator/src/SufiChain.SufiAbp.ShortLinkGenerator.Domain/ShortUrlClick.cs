using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public class ShortUrlClick : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    public Guid ShortUrlId { get; set; }
    
    public DateTime ClickedAt { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? Referrer { get; set; }
    
    protected ShortUrlClick()
    {
    }
    
    public ShortUrlClick(
        Guid id,
        Guid shortUrlId,
        DateTime clickedAt,
        string? userAgent = null,
        string? ipAddress = null,
        string? referrer = null)
        : base(id)
    {
        ShortUrlId = shortUrlId;
        ClickedAt = clickedAt;
        UserAgent = userAgent;
        IpAddress = ipAddress;
        Referrer = referrer;
    }
}


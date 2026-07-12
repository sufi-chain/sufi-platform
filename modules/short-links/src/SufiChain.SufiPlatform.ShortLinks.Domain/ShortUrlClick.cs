using System;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.ShortLinks;

public class ShortUrlClick : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    
    public Guid ShortUrlId { get; set; }
    
    public DateTime ClickedAt { get; set; }
    
    public string? UserAgent { get; set; }
    
    public string? IpAddress { get; set; }
    
    public string? Referrer { get; set; }

    /// <summary>The 'c' query param (contact token) that accompanied this click, if any.</summary>
    public string? Token { get; set; }

    /// <summary>Stable dedup key, e.g. ShortUrlId:Token or ShortUrlId:IpAddress when no token.</summary>
    public string? DedupKey { get; set; }
    
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


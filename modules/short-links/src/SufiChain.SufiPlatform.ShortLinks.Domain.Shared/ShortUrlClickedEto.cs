using System;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// A successful public redirect. Every caller is counted, including the link owner
/// and an authenticated user. The outbox delivers this after the redirect unit of work.
/// </summary>
public class ShortUrlClickedEto : IMultiTenant
{
    public Guid ShortUrlId { get; set; }

    public Guid? TenantId { get; set; }

    public DateTime ClickedAt { get; set; }

    public string? UserAgent { get; set; }

    public string? IpAddress { get; set; }

    public string? Referrer { get; set; }

    public string? Token { get; set; }

    public string? DedupKey { get; set; }
}

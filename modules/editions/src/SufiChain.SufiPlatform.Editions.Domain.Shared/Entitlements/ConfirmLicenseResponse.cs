using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Editions.Entitlements;

/// <summary>
/// License API confirm payload for a future On-prem host.
/// Not wired in the SaaS host; contracts only.
/// </summary>
public class ConfirmLicenseResponse
{
    public Guid TenantId { get; set; }

    public string EditionCode { get; set; } = string.Empty;

    public Dictionary<string, string> Features { get; set; } = new();

    public DateTimeOffset ExpiresAt { get; set; }

    public DateTimeOffset? GraceUntil { get; set; }
}

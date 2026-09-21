using System;

namespace SufiChain.SufiPlatform.OpenIddict;

/// <summary>
/// Periodic prune of expired OpenIddict tokens and authorizations.
/// Tokens are host-owned and are not <c>IMultiTenant</c>.
/// </summary>
public class OpenIddictCleanupOptions
{
    public bool IsCleanupEnabled { get; set; } = true;

    public bool DisableTokenCleanup { get; set; }

    public bool DisableAuthorizationCleanup { get; set; }

    /// <summary>
    /// Worker period. Default is four hours.
    /// </summary>
    public int CleanupPeriodMilliseconds { get; set; } = 4 * 60 * 60 * 1000;

    /// <summary>
    /// OpenIddict prune threshold. Entries older than this age are eligible.
    /// </summary>
    public TimeSpan MinimumAge { get; set; } = TimeSpan.FromDays(14);
}

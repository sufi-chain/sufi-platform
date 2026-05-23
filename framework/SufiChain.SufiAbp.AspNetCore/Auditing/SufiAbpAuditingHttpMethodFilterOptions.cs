using System.Collections.Generic;

namespace SufiChain.SufiAbp.AspNetCore.Auditing;

/// <summary>
/// Configurable HTTP method filter for audit logging. When not configured, all requests are audited
/// except those in the blacklist. Use whitelist for strict "only these methods" control.
/// </summary>
public class SufiAbpAuditingHttpMethodFilterOptions
{
    /// <summary>
    /// HTTP methods to exclude from audit logging. If the request method is in this list, the request
    /// will not be audited. Default: ["GET", "HEAD"] — GET/HEAD are typically read-only and noisy.
    /// </summary>
    public List<string> BlacklistedHttpMethods { get; } = new() { "GET", "HEAD" };

    /// <summary>
    /// When set and not empty, ONLY these HTTP methods are audited. All others are excluded.
    /// Takes precedence over <see cref="BlacklistedHttpMethods"/>. Leave null to use blacklist only.
    /// Example: ["POST", "PUT", "DELETE", "PATCH"] to audit only modifying operations.
    /// </summary>
    public List<string>? WhitelistedHttpMethods { get; set; }

    /// <summary>
    /// Clears the default blacklist (GET, HEAD) so all methods are audited unless whitelist is set.
    /// Call this if you want GET requests audited by default.
    /// </summary>
    public void ClearDefaultBlacklist()
    {
        BlacklistedHttpMethods.Clear();
    }
}

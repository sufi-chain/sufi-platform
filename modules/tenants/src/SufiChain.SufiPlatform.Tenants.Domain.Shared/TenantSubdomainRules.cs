using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SufiChain.SufiPlatform.Tenants;

/// <summary>
/// Strict, Latin-only rules for a workspace (tenant) subdomain.
/// A subdomain becomes part of the generated host name and the tenant database name,
/// so it must be an ASCII DNS label and must never collide with platform infrastructure.
/// </summary>
public static class TenantSubdomainRules
{
    public const int MinLength = 3;
    public const int MaxLength = 63;

    /// <summary>
    /// Lowercase ASCII letters, digits, and hyphens only. Must start and end with
    /// an alphanumeric character and must not contain consecutive hyphens in the
    /// third and fourth positions (blocks <c>xn--</c> punycode labels).
    /// </summary>
    public static readonly Regex Pattern = new(
        "^[a-z0-9](?:[a-z0-9]|-(?=[a-z0-9])){0,61}[a-z0-9]$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    /// <summary>
    /// Reserved platform and infrastructure subdomains that a tenant may never claim.
    /// </summary>
    public static readonly IReadOnlySet<string> ReservedSubdomains = new HashSet<string>(
        new[]
        {
            "console", "pc", "stage", "harbor", "www", "api", "gw", "admin",
            "mail", "smtp", "ftp", "cdn", "static", "assets", "app", "auth",
            "id", "sso", "status", "docs", "help", "support", "blog", "dev",
            "test", "staging", "prod"
        },
        StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Validates the raw subdomain value. When valid, returns the normalized
    /// (trimmed, lowercased) subdomain; otherwise returns <c>null</c> and sets
    /// <paramref name="reason"/> to the failing rule.
    /// </summary>
    public static bool TryValidate(string? subdomain, out string normalized, out string reason)
    {
        normalized = subdomain?.Trim().ToLowerInvariant() ?? string.Empty;
        reason = string.Empty;

        if (normalized.Length == 0)
        {
            reason = "Required";
            return false;
        }

        if (normalized.Length is < MinLength or > MaxLength)
        {
            reason = "Length";
            return false;
        }

        if (normalized.StartsWith("-", StringComparison.Ordinal) ||
            normalized.EndsWith("-", StringComparison.Ordinal))
        {
            reason = "EdgeHyphen";
            return false;
        }

        if (normalized.Length >= 4 &&
            normalized[2] == '-' &&
            normalized[3] == '-')
        {
            reason = "Punycode";
            return false;
        }

        if (!Pattern.IsMatch(normalized))
        {
            reason = "Characters";
            return false;
        }

        if (ReservedSubdomains.Contains(normalized))
        {
            reason = "Reserved";
            return false;
        }

        return true;
    }
}

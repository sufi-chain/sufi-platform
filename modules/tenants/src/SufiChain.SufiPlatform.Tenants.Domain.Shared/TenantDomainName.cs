using System;
using System.Globalization;
using System.Linq;

namespace SufiChain.SufiPlatform.Tenants;

public static class TenantDomainName
{
    public static string NormalizeHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            throw new ArgumentException("Tenant domain host can not be empty.", nameof(host));
        }

        if (!string.Equals(host, host.Trim(), StringComparison.Ordinal) ||
            host.EndsWith(".", StringComparison.Ordinal) ||
            host.Contains("://", StringComparison.Ordinal) ||
            host.IndexOfAny(['/', '\\', ':', '*', '?', '#', '@']) >= 0)
        {
            throw new ArgumentException("Tenant domain host is invalid.", nameof(host));
        }

        string normalizedHost;
        try
        {
            var idnMapping = new IdnMapping();
            normalizedHost = string.Join(
                ".",
                host.Split('.').Select(label => idnMapping.GetAscii(label).ToLowerInvariant()));
        }
        catch (ArgumentException)
        {
            throw new ArgumentException("Tenant domain host is invalid.", nameof(host));
        }

        if (normalizedHost.Length > TenantConsts.MaxDomainHostLength ||
            Uri.CheckHostName(normalizedHost) != UriHostNameType.Dns ||
            normalizedHost.Split('.').Any(label =>
                label.Length is 0 or > 63 ||
                label.StartsWith("-", StringComparison.Ordinal) ||
                label.EndsWith("-", StringComparison.Ordinal) ||
                label.Any(character => !IsAsciiLetterOrDigit(character) && character != '-')))
        {
            throw new ArgumentException("Tenant domain host is invalid.", nameof(host));
        }

        return normalizedHost;
    }

    public static bool TryNormalizeHost(string? host, out string normalizedHost)
    {
        try
        {
            normalizedHost = NormalizeHost(host!);
            return true;
        }
        catch (ArgumentException)
        {
            normalizedHost = string.Empty;
            return false;
        }
    }

    public static string NormalizeSubdomain(string subdomain)
    {
        if (string.IsNullOrWhiteSpace(subdomain))
        {
            throw new ArgumentException("Tenant subdomain can not be empty.", nameof(subdomain));
        }

        var normalized = new IdnMapping().GetAscii(subdomain.Trim()).ToLowerInvariant();
        if (normalized.Length > TenantConsts.MaxSubdomainLength ||
            normalized.StartsWith("-", StringComparison.Ordinal) ||
            normalized.EndsWith("-", StringComparison.Ordinal) ||
            normalized.Any(character =>
                !IsAsciiLetterOrDigit(character) &&
                character != '-'))
        {
            throw new ArgumentException("Tenant subdomain is invalid.", nameof(subdomain));
        }

        return normalized;
    }

    private static bool IsAsciiLetterOrDigit(char character)
    {
        return character is >= 'a' and <= 'z' or >= 'A' and <= 'Z' or >= '0' and <= '9';
    }
}

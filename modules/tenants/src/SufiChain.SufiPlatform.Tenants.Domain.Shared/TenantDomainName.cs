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

        host = host.Trim();
        if (Uri.TryCreate(host, UriKind.Absolute, out var absoluteUri))
        {
            host = absoluteUri.Host;
        }
        else
        {
            var slashIndex = host.IndexOf('/');
            if (slashIndex >= 0)
            {
                host = host[..slashIndex];
            }

            var colonIndex = host.LastIndexOf(':');
            if (colonIndex > 0 && host.IndexOf(':') == colonIndex)
            {
                host = host[..colonIndex];
            }
        }

        host = host.Trim().TrimEnd('.');
        if (host.Length == 0 || host.Contains('*'))
        {
            throw new ArgumentException("Tenant domain host is invalid.", nameof(host));
        }

        var idnMapping = new IdnMapping();
        var normalizedHost = string.Join(
            ".",
            host.Split('.').Select(label => idnMapping.GetAscii(label).ToLowerInvariant()));

        if (normalizedHost.Length > TenantConsts.MaxDomainHostLength ||
            Uri.CheckHostName(normalizedHost) != UriHostNameType.Dns)
        {
            throw new ArgumentException("Tenant domain host is invalid.", nameof(host));
        }

        return normalizedHost;
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

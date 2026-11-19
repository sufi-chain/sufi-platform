using System;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Public short links are <c>{tenant-public-origin}/s/{code}</c>.
/// The console application root is not a public origin.
/// </summary>
public static class ShortLinkPublicUrl
{
    public const string Segment = "s";

    public const string PublicApplicationName = "Public";

    public static string Build(string? origin, string? shortCode)
    {
        var code = (shortCode ?? string.Empty).Trim().Trim('/');
        var root = string.IsNullOrWhiteSpace(origin) ? string.Empty : origin.Trim().TrimEnd('/');
        return string.IsNullOrEmpty(root)
            ? "/" + Segment + "/" + code
            : root + "/" + Segment + "/" + code;
    }

    /// <summary>
    /// A dedicated or generated tenant host replaces the public origin's host and keeps its scheme and port.
    /// <paramref name="publicApplicationRoot"/> is the ABP <c>AppUrlOptions</c> public application root.
    /// A <c>{0}</c> placeholder is the tenant subdomain when no host is stored.
    /// </summary>
    public static string? ResolveOrigin(
        string? publicApplicationRoot,
        string? publicHostUrl,
        string? openIddictPublicRoot,
        string? tenantHost,
        string? tenantSubdomain)
    {
        var template = First(publicApplicationRoot, publicHostUrl, openIddictPublicRoot);
        if (!string.IsNullOrWhiteSpace(tenantHost))
        {
            return Origin(tenantHost, WithoutTenantPlaceholder(template));
        }

        if (!string.IsNullOrWhiteSpace(tenantSubdomain) &&
            template != null &&
            template.Contains("{0}", StringComparison.Ordinal))
        {
            return Origin(template.Replace("{0}", tenantSubdomain.Trim(), StringComparison.Ordinal), template);
        }

        return Origin(WithoutTenantPlaceholder(template), WithoutTenantPlaceholder(template));
    }

    public static string? Origin(string? value, string? schemeTemplate)
    {
        if (!IsSafe(value))
        {
            return null;
        }

        var trimmed = value!.Trim();
        if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute) && IsHttp(absolute) && !absolute.Host.Contains('{'))
        {
            return absolute.GetLeftPart(UriPartial.Authority);
        }

        if (!IsSafe(schemeTemplate) ||
            !Uri.TryCreate(WithoutTenantPlaceholder(schemeTemplate) ?? string.Empty, UriKind.Absolute, out var template) ||
            !IsHttp(template) ||
            trimmed.IndexOfAny(new[] { '/', '\\', '?', '#', '@', '{', '}' }) >= 0)
        {
            return null;
        }

        var port = template.IsDefaultPort ? string.Empty : ":" + template.Port.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var origin = template.Scheme + "://" + trimmed + port;
        return Uri.TryCreate(origin, UriKind.Absolute, out var built) && IsHttp(built)
            ? built.GetLeftPart(UriPartial.Authority)
            : null;
    }

    private static string? First(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? WithoutTenantPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("{0}", StringComparison.Ordinal))
        {
            return value;
        }

        return value.Replace("{0}.", string.Empty, StringComparison.Ordinal).Replace("{0}", string.Empty, StringComparison.Ordinal);
    }

    private static bool IsSafe(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.IndexOfAny(new[] { '*', ' ', '\r', '\n', ';', ',', '\'', '"', '\\' }) < 0;
    }

    private static bool IsHttp(Uri uri) =>
        uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
}

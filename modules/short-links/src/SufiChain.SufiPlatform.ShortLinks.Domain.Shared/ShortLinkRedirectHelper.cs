using System;
using System.Linq;

namespace SufiChain.SufiPlatform.ShortLinks;

/// <summary>
/// Builds the final redirect destination for a short link, forwarding the
/// per-contact token ('c' query param) so the landing page can resolve the contact.
/// </summary>
public static class ShortLinkRedirectHelper
{
    /// <summary>
    /// Normalizes the configured public redirect base key to a single URL segment.
    /// </summary>
    public static string NormalizeBaseKey(string? baseKey)
    {
        var normalizedBaseKey = baseKey?.Trim().Trim('/');
        if (string.IsNullOrWhiteSpace(normalizedBaseKey))
        {
            return ShortLinksConsts.DefaultRedirectRoute;
        }

        return normalizedBaseKey.Contains('/')
               || normalizedBaseKey.Contains('\\')
               || normalizedBaseKey.Any(char.IsWhiteSpace)
            ? ShortLinksConsts.DefaultRedirectRoute
            : normalizedBaseKey;
    }

    /// <summary>
    /// Appends the incoming 'c' token to the destination unless the destination
    /// already contains it (templated destinations include '{InvitationToken}').
    /// </summary>
    /// <summary>
    /// A destination may be stored only when it is an absolute http or https URL.
    /// Anything else, including a value already in the database, must not be followed.
    /// </summary>
    public static bool IsAbsoluteHttpUrl(string? destination)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return false;
        }

        var value = destination.Trim();
        if (value.IndexOf('\r') >= 0 || value.IndexOf('\n') >= 0 || value.IndexOf('\0') >= 0)
        {
            return false;
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
        {
            return false;
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return !string.IsNullOrEmpty(uri.Host);
    }

    public static string AppendToken(string destination, string? token)
    {
        if (string.IsNullOrWhiteSpace(destination))
        {
            return destination;
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            return destination;
        }

        // Templated destinations are resolved before storage (no '{InvitationToken}' here at runtime).
        if (destination.Contains("{InvitationToken}", StringComparison.OrdinalIgnoreCase))
        {
            return destination.ReplaceFirst("{InvitationToken}", token, StringComparison.OrdinalIgnoreCase);
        }

        if (destination.Contains("c=", StringComparison.OrdinalIgnoreCase) ||
            destination.Contains("invitation=", StringComparison.OrdinalIgnoreCase))
        {
            return destination;
        }

        return destination + (destination.Contains('?') ? "&" : "?") + "c=" + Uri.EscapeDataString(token);
    }
}

using System;
using System.Linq;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

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
            return ShortLinkGeneratorConsts.DefaultRedirectRoute;
        }

        return normalizedBaseKey.Contains('/')
               || normalizedBaseKey.Contains('\\')
               || normalizedBaseKey.Any(char.IsWhiteSpace)
            ? ShortLinkGeneratorConsts.DefaultRedirectRoute
            : normalizedBaseKey;
    }

    /// <summary>
    /// Appends the incoming 'c' token to the destination unless the destination
    /// already contains it (templated destinations include '{InvitationToken}').
    /// </summary>
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

        if (destination.Contains("c=", StringComparison.OrdinalIgnoreCase))
        {
            return destination;
        }

        return destination + (destination.Contains('?') ? "&" : "?") + "c=" + Uri.EscapeDataString(token);
    }
}

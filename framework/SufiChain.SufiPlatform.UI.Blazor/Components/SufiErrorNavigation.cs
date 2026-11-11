namespace SufiChain.SufiPlatform.UI.Blazor.Components;

/// <summary>
/// Chooses the recovery URL for an error page when the caller did not pass one.
/// </summary>
public static class SufiErrorNavigation
{
    public const string PanelHome = "/panel/dashboard";
    public const string SiteHome = "/";

    public static string ResolveHomeUrl(string? requestedHomeUrl, string? currentUri)
    {
        if (!string.IsNullOrWhiteSpace(requestedHomeUrl))
        {
            return requestedHomeUrl;
        }

        return IsPanelContext(currentUri) ? PanelHome : SiteHome;
    }

    private static bool IsPanelContext(string? currentUri)
    {
        if (string.IsNullOrWhiteSpace(currentUri))
        {
            return false;
        }

        if (!Uri.TryCreate(currentUri, UriKind.Absolute, out var absolute))
        {
            absolute = new Uri(new Uri("https://sufi.local"), currentUri);
        }

        var path = absolute.AbsolutePath;
        if (path.Equals("/panel", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/panel/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var from = ReadQuery(absolute, "from");
        if (string.IsNullOrEmpty(from))
        {
            return false;
        }

        if (Uri.TryCreate(from, UriKind.Absolute, out var fromAbsolute))
        {
            from = fromAbsolute.AbsolutePath;
        }

        var queryIndex = from.IndexOfAny(['?', '#']);
        if (queryIndex >= 0)
        {
            from = from[..queryIndex];
        }

        return from.Equals("/panel", StringComparison.OrdinalIgnoreCase)
               || from.StartsWith("/panel/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ReadQuery(Uri uri, string key)
    {
        var query = uri.Query.TrimStart('?');
        if (query.Length == 0)
        {
            return null;
        }

        foreach (var pair in query.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var separatorIndex = pair.IndexOf('=');
            var name = separatorIndex >= 0 ? pair[..separatorIndex] : pair;
            if (!name.Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var encoded = separatorIndex >= 0 ? pair[(separatorIndex + 1)..] : string.Empty;
            return Uri.UnescapeDataString(encoded.Replace("+", " ", StringComparison.Ordinal));
        }

        return null;
    }
}

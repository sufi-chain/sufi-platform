namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

/// <summary>
/// Links into the shared <c>/panel/admin/settings</c> shell.
/// The shell selects a section from <c>?section=</c> and keeps any other query parameters.
/// Moved pages (#674) use orders 120, 130, 140 and 150.
/// Sections added beside them should use a different order.
/// </summary>
public static class SettingsSectionLink
{
    public const string PagePath = "/panel/admin/settings";

    public const string SectionParameter = "section";

    public const string TabParameter = "tab";

    public static string For(string sectionId, string? tab = null)
    {
        var query = $"{SectionParameter}={Uri.EscapeDataString(sectionId)}";
        if (!string.IsNullOrWhiteSpace(tab))
        {
            query += $"&{TabParameter}={Uri.EscapeDataString(tab)}";
        }

        return $"{PagePath}?{query}";
    }

    /// <summary>
    /// Sends an old settings URL to the shell section, keeping culture and return parameters.
    /// </summary>
    public static string Redirect(string absoluteUri, string sectionId, string? tab = null)
    {
        var kept = new List<string>();
        var query = new Uri(absoluteUri).Query.TrimStart('?');
        if (query.Length > 0)
        {
            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var name = Uri.UnescapeDataString(part.Split('=', 2)[0]);
                if (name.Equals(SectionParameter, StringComparison.OrdinalIgnoreCase)
                    || name.Equals(TabParameter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                kept.Add(part);
            }
        }

        var target = For(sectionId, tab);
        return kept.Count == 0 ? target : target + "&" + string.Join("&", kept);
    }

    public static string? ReadTab(string absoluteUri)
    {
        var query = new Uri(absoluteUri).Query.TrimStart('?');
        if (query.Length == 0)
        {
            return null;
        }

        foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var pieces = part.Split('=', 2);
            if (pieces.Length == 2
                && Uri.UnescapeDataString(pieces[0]).Equals(TabParameter, StringComparison.OrdinalIgnoreCase))
            {
                var tab = Uri.UnescapeDataString(pieces[1]);
                return string.IsNullOrWhiteSpace(tab) ? null : tab;
            }
        }

        return null;
    }
}

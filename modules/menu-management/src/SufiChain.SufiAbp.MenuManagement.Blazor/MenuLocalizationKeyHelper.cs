using System.Text.RegularExpressions;
using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.MenuManagement.Blazor;

public static partial class MenuLocalizationKeyHelper
{
    public static string NormalizeMenuKey(string? contextType, string? name)
    {
        var contextSegment = NormalizeSegment(contextType);
        var nameSegment = NormalizeSegment(name);

        if (string.IsNullOrEmpty(contextSegment))
        {
            return nameSegment;
        }

        if (string.IsNullOrEmpty(nameSegment))
        {
            return contextSegment;
        }

        return $"{contextSegment}-{nameSegment}";
    }

    public static string NormalizeItemSlug(string? slug, string? name)
    {
        if (!string.IsNullOrWhiteSpace(slug))
        {
            return NormalizeSegment(slug);
        }

        return NormalizeSegment(name);
    }

    public static string? ResolveMenuKey(string? displayName, string? contextType, string? name)
    {
        if (BusinessLocalizationHelper.TryExtractSeededMenuKey(displayName ?? string.Empty, out var menuKey))
        {
            return menuKey;
        }

        var normalized = NormalizeMenuKey(contextType, name);
        return string.IsNullOrWhiteSpace(normalized) ? null : normalized;
    }

    public static string NormalizeSegment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim().ToLowerInvariant();
        normalized = normalized.Replace('_', '-').Replace(' ', '-');
        normalized = InvalidSegmentCharacters().Replace(normalized, string.Empty);
        normalized = DuplicateHyphens().Replace(normalized, "-").Trim('-');

        return normalized;
    }

    [GeneratedRegex(@"[^a-z0-9\-\.]", RegexOptions.CultureInvariant)]
    private static partial Regex InvalidSegmentCharacters();

    [GeneratedRegex(@"-{2,}", RegexOptions.CultureInvariant)]
    private static partial Regex DuplicateHyphens();
}

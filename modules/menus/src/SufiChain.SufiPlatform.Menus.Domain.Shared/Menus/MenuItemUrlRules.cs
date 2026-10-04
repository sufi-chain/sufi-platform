using System.Text;

namespace SufiChain.SufiPlatform.Menus.Menus;

/// <summary>
/// Accepts root-relative paths, in-page anchors, and http, https, mailto, and tel URLs.
/// Rejects javascript and every other scheme. Culture overrides are stored as text in
/// <see cref="CultureUrlsPropertyName"/> and fall back to the default URL.
/// </summary>
public static class MenuItemUrlRules
{
    public const string CultureUrlsPropertyName = "CultureUrls";

    public const int MaxCultureUrlOverrides = 32;

    public static bool IsAcceptable(MenuItemKind kind, string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return kind is not MenuItemKind.InternalRoute and not MenuItemKind.ExternalUrl;
        }

        return Normalize(url) != null;
    }

    public static string? Normalize(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        var value = url.Trim();
        if (value.Length > MenusConsts.MaxUrlLength || HasForbiddenCharacters(value))
        {
            return null;
        }

        if (value[0] == '#')
        {
            return value;
        }

        if (value[0] == '/')
        {
            return value.StartsWith("//", StringComparison.Ordinal) ? null : value;
        }

        var schemeSeparator = value.IndexOf(':');
        if (schemeSeparator <= 0)
        {
            return null;
        }

        var scheme = value.Substring(0, schemeSeparator);
        if (!IsAllowedScheme(scheme))
        {
            return null;
        }

        var canonicalScheme = scheme.ToLowerInvariant();
        var remainder = value.Substring(schemeSeparator + 1);
        if (remainder.Length == 0)
        {
            return null;
        }

        if (canonicalScheme is "http" or "https")
        {
            if (!remainder.StartsWith("//", StringComparison.Ordinal))
            {
                return null;
            }

            if (!Uri.TryCreate(canonicalScheme + ":" + remainder, UriKind.Absolute, out var absolute)
                || !IsAllowedScheme(absolute.Scheme)
                || string.IsNullOrEmpty(absolute.Host))
            {
                return null;
            }

            return canonicalScheme + ":" + remainder;
        }

        if (remainder.StartsWith("//", StringComparison.Ordinal))
        {
            return null;
        }

        if (!Uri.TryCreate(canonicalScheme + ":" + remainder, UriKind.Absolute, out var uri)
            || !string.Equals(uri.Scheme, canonicalScheme, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return canonicalScheme + ":" + remainder;
    }

    public static bool IsExternal(string? normalizedUrl)
    {
        if (string.IsNullOrEmpty(normalizedUrl))
        {
            return false;
        }

        return normalizedUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || normalizedUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || normalizedUrl.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || normalizedUrl.StartsWith("tel:", StringComparison.OrdinalIgnoreCase);
    }

    public static string? Resolve(string? defaultUrl, IReadOnlyDictionary<string, string>? cultureUrls, string? culture)
    {
        var selected = SelectOverride(cultureUrls, culture);
        if (selected != null)
        {
            var normalized = Normalize(selected);
            if (normalized != null)
            {
                return normalized;
            }
        }

        return Normalize(defaultUrl);
    }

    public static bool TryNormalizeCultureUrls(
        IEnumerable<KeyValuePair<string, string>>? cultureUrls,
        out Dictionary<string, string> normalized)
    {
        normalized = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (cultureUrls == null)
        {
            return true;
        }

        foreach (var pair in cultureUrls)
        {
            if (string.IsNullOrWhiteSpace(pair.Value))
            {
                continue;
            }

            var culture = CanonicalCulture(pair.Key);
            var url = Normalize(pair.Value);
            if (culture == null || url == null)
            {
                normalized.Clear();
                return false;
            }

            normalized[culture] = url;
            if (normalized.Count > MaxCultureUrlOverrides)
            {
                normalized.Clear();
                return false;
            }
        }

        return true;
    }

    public static bool TryParseCultureUrls(string? text, out Dictionary<string, string> cultureUrls)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            cultureUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            return true;
        }

        var pairs = new List<KeyValuePair<string, string>>();
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            var split = IndexOfWhitespace(line);
            if (split <= 0)
            {
                cultureUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return false;
            }

            var culture = line.Substring(0, split);
            var url = line.Substring(split).Trim();
            if (url.Length == 0)
            {
                cultureUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return false;
            }

            pairs.Add(new KeyValuePair<string, string>(culture, url));
        }

        return TryNormalizeCultureUrls(pairs, out cultureUrls);
    }

    public static string FormatCultureUrls(IReadOnlyDictionary<string, string>? cultureUrls)
    {
        if (cultureUrls == null || cultureUrls.Count == 0)
        {
            return string.Empty;
        }

        var lines = new List<string>();
        foreach (var pair in cultureUrls.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
        {
            var culture = CanonicalCulture(pair.Key);
            var url = Normalize(pair.Value);
            if (culture == null || url == null)
            {
                continue;
            }

            lines.Add(culture + " " + url);
        }

        return string.Join("\n", lines);
    }

    public static string? SerializeCultureUrls(IReadOnlyDictionary<string, string>? cultureUrls)
    {
        var text = FormatCultureUrls(cultureUrls);
        return text.Length == 0 ? null : text;
    }

    public static IReadOnlyDictionary<string, string> DeserializeCultureUrls(string? text)
    {
        return TryParseCultureUrls(text, out var cultureUrls)
            ? cultureUrls
            : new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    }

    public static string? CanonicalCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        var parts = culture.Trim().Replace('_', '-').Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length is < 1 or > 3)
        {
            return null;
        }

        if (!IsAsciiLetters(parts[0]) || parts[0].Length is < 2 or > 3)
        {
            return null;
        }

        var builder = new StringBuilder(parts[0].ToLowerInvariant());
        for (var i = 1; i < parts.Length; i++)
        {
            var part = parts[i];
            string canonical;
            if (IsAsciiLetters(part) && part.Length == 4)
            {
                canonical = char.ToUpperInvariant(part[0]) + part.Substring(1).ToLowerInvariant();
            }
            else if (IsAsciiLetters(part) && part.Length == 2)
            {
                canonical = part.ToUpperInvariant();
            }
            else if (IsAsciiDigits(part) && part.Length == 3)
            {
                canonical = part;
            }
            else
            {
                return null;
            }

            builder.Append('-').Append(canonical);
        }

        return builder.ToString();
    }

    private static string? SelectOverride(IReadOnlyDictionary<string, string>? cultureUrls, string? culture)
    {
        if (cultureUrls == null || cultureUrls.Count == 0)
        {
            return null;
        }

        var candidate = CanonicalCulture(culture);
        if (candidate == null)
        {
            return null;
        }

        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var pair in cultureUrls)
        {
            if (string.IsNullOrWhiteSpace(pair.Value))
            {
                continue;
            }

            var key = CanonicalCulture(pair.Key);
            if (key != null)
            {
                map[key] = pair.Value;
            }
        }

        while (candidate != null)
        {
            if (map.TryGetValue(candidate, out var url) && !string.IsNullOrWhiteSpace(url))
            {
                return url;
            }

            var dash = candidate.LastIndexOf('-');
            candidate = dash > 0 ? candidate.Substring(0, dash) : null;
        }

        return null;
    }

    private static bool IsAllowedScheme(string scheme) =>
        scheme.Equals("http", StringComparison.OrdinalIgnoreCase)
        || scheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        || scheme.Equals("mailto", StringComparison.OrdinalIgnoreCase)
        || scheme.Equals("tel", StringComparison.OrdinalIgnoreCase);

    private static bool HasForbiddenCharacters(string value)
    {
        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch) || char.IsControl(ch) || ch is '\\' or '<' or '>' or '"' or '\'')
            {
                return true;
            }
        }

        return false;
    }

    private static int IndexOfWhitespace(string value)
    {
        for (var i = 0; i < value.Length; i++)
        {
            if (char.IsWhiteSpace(value[i]))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsAsciiLetters(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        foreach (var ch in value)
        {
            if (ch is < 'A' or > 'Z' and < 'a' or > 'z')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsAsciiDigits(string value)
    {
        if (value.Length == 0)
        {
            return false;
        }

        foreach (var ch in value)
        {
            if (ch is < '0' or > '9')
            {
                return false;
            }
        }

        return true;
    }
}

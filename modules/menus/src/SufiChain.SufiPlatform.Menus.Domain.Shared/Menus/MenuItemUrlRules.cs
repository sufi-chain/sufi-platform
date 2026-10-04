using System.Globalization;
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

        return kind switch
        {
            MenuItemKind.InternalRoute => IsInternalUrl(Normalize(url)),
            MenuItemKind.ExternalUrl => IsExternal(Normalize(url)),
            _ => Normalize(url) != null
        };
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

    /// <summary>
    /// A stored <c>#id</c> or root <c>/#id</c> is a section of the page the visitor is on.
    /// It is rendered on <paramref name="currentPath"/> so <c>&lt;base href="/"&gt;</c> does not
    /// send it to the unprefixed root, and so it does not jump to that culture's home page.
    /// </summary>
    public static string? ToPublicHref(string? url, string? currentPath)
    {
        var normalized = Normalize(url);
        if (normalized == null)
        {
            return null;
        }

        if (normalized[0] == '#' || normalized.StartsWith("/#", StringComparison.Ordinal))
        {
            return TryReadRootSection(normalized, out var sectionId)
                ? CurrentPagePath(currentPath) + "#" + sectionId
                : null;
        }

        return normalized;
    }

    /// <summary>
    /// True for <c>#id</c> and <c>/#id</c>. A path such as <c>/about#id</c> is a section of that page.
    /// </summary>
    public static bool TryReadRootSection(string? url, out string sectionId)
    {
        sectionId = string.Empty;
        var normalized = Normalize(url);
        if (normalized == null)
        {
            return false;
        }

        if (normalized[0] == '#')
        {
            sectionId = normalized.Substring(1);
            return IsSectionId(sectionId);
        }

        if (normalized.StartsWith("/#", StringComparison.Ordinal))
        {
            sectionId = normalized.Substring(2);
            return IsSectionId(sectionId);
        }

        return false;
    }

    /// <summary>
    /// A section link is the current page only when its path and fragment both match.
    /// A link without a fragment matches the path alone.
    /// </summary>
    public static bool IsPageCurrent(string? linkHref, string? currentPath, string? currentFragment)
    {
        if (string.IsNullOrWhiteSpace(linkHref))
        {
            return false;
        }

        var hash = linkHref.IndexOf('#');
        var linkPath = hash switch
        {
            < 0 => linkHref,
            0 => "/",
            _ => linkHref.Substring(0, hash)
        };

        if (!PagePathsEqual(linkPath, CurrentPagePath(currentPath)))
        {
            return false;
        }

        if (hash < 0)
        {
            return true;
        }

        var linkFragment = linkHref.Substring(hash + 1);
        return linkFragment.Length > 0
            && string.Equals(linkFragment, currentFragment ?? string.Empty, StringComparison.Ordinal);
    }

    private static string CurrentPagePath(string? currentPath)
    {
        var path = string.IsNullOrWhiteSpace(currentPath) ? "/" : currentPath.Trim();
        var cut = path.IndexOfAny(new[] { '?', '#' });
        if (cut >= 0)
        {
            path = path.Substring(0, cut);
        }

        if (!path.StartsWith("/", StringComparison.Ordinal))
        {
            path = "/" + path;
        }

        if (path.Length > 1)
        {
            path = path.TrimEnd('/');
        }

        return path.Length == 0 ? "/" : path;
    }

    private static bool PagePathsEqual(string left, string right)
    {
        var a = CurrentPagePath(left);
        var b = CurrentPagePath(right);
        if (string.Equals(a, b, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return (a == "/" && b.Length == 0) || (b == "/" && a.Length == 0);
    }

    public static bool TryNormalizeSitePage(string? input, out string? url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var value = input.Trim();
        if (!value.StartsWith("/", StringComparison.Ordinal) || value.Contains('#'))
        {
            return false;
        }

        url = Normalize(value);
        return IsInternalUrl(url);
    }

    public static bool TryNormalizeSection(bool currentPage, string? pagePath, string? sectionId, out string? url)
    {
        url = null;
        var id = sectionId?.Trim() ?? string.Empty;
        if (!IsSectionId(id))
        {
            return false;
        }

        if (currentPage || string.IsNullOrWhiteSpace(pagePath))
        {
            url = "#" + id;
            return true;
        }

        var path = pagePath.Trim();
        if (!TryNormalizeSitePage(path, out var page))
        {
            return false;
        }

        url = page + "#" + id;
        return true;
    }

    public static bool TryNormalizeWebsite(string? input, out string? url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var value = ToLatinDigits(input.Trim());
        if (value.StartsWith("/", StringComparison.Ordinal)
            || value.StartsWith("#", StringComparison.Ordinal)
            || value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("tel:", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var candidate = value.Contains("://", StringComparison.Ordinal) ? value : "https://" + value;
        url = Normalize(candidate);
        return url != null && (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase));
    }

    public static bool TryNormalizeEmail(string? input, out string? url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var value = ToLatinDigits(input.Trim());
        if (value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring("mailto:".Length);
        }

        if (value.Length == 0 || value.IndexOf('@') <= 0 || value.IndexOf('@') != value.LastIndexOf('@'))
        {
            return false;
        }

        url = Normalize("mailto:" + value);
        return url != null;
    }

    public static bool TryNormalizePhone(string? input, out string? url)
    {
        url = null;
        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var value = ToLatinDigits(input.Trim());
        if (value.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            value = value.Substring("tel:".Length);
        }

        var builder = new StringBuilder(value.Length);
        var digits = 0;
        foreach (var ch in value)
        {
            if (ch is >= '0' and <= '9')
            {
                builder.Append(ch);
                digits++;
                continue;
            }

            if (ch is '+' or '-' or '(' or ')' || char.IsWhiteSpace(ch))
            {
                if (!char.IsWhiteSpace(ch))
                {
                    builder.Append(ch);
                }

                continue;
            }

            return false;
        }

        if (digits < 3)
        {
            return false;
        }

        url = Normalize("tel:" + builder);
        return url != null;
    }

    public static string? NormalizeCultureOverride(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return null;
        }

        var value = input.Trim();
        if (value[0] is '/' or '#')
        {
            return IsAcceptable(MenuItemKind.InternalRoute, value) ? Normalize(value) : null;
        }

        if (value.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || (value.IndexOf('@') > 0 && value.IndexOf("://", StringComparison.Ordinal) < 0))
        {
            return TryNormalizeEmail(value, out var email) ? email : null;
        }

        if (value.StartsWith("tel:", StringComparison.OrdinalIgnoreCase) || LooksLikePhone(value))
        {
            return TryNormalizePhone(value, out var phone) ? phone : null;
        }

        return TryNormalizeWebsite(value, out var website) ? website : null;
    }

    public static bool TryParseDisplayOrder(string? text, out int displayOrder)
    {
        displayOrder = 0;
        if (string.IsNullOrWhiteSpace(text))
        {
            return false;
        }

        return int.TryParse(ToLatinDigits(text.Trim()), NumberStyles.Integer, CultureInfo.InvariantCulture, out displayOrder);
    }

    public static string ToLatinDigits(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch is >= '\u06F0' and <= '\u06F9')
            {
                builder.Append((char)('0' + (ch - '\u06F0')));
            }
            else if (ch is >= '\u0660' and <= '\u0669')
            {
                builder.Append((char)('0' + (ch - '\u0660')));
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    public static bool IsSectionId(string? id)
    {
        if (string.IsNullOrEmpty(id) || id![0] is < 'A' or > 'Z' and < 'a' or > 'z')
        {
            return false;
        }

        for (var i = 1; i < id.Length; i++)
        {
            var ch = id[i];
            if (ch is (< 'A' or > 'Z') and (< 'a' or > 'z') and (< '0' or > '9') and not '_' and not '-')
            {
                return false;
            }
        }

        return true;
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
            var url = NormalizeCultureOverride(pair.Value);
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

    private static bool IsInternalUrl(string? normalized)
    {
        if (string.IsNullOrEmpty(normalized) || IsExternal(normalized))
        {
            return false;
        }

        if (normalized![0] == '#')
        {
            return IsSectionId(normalized.Substring(1));
        }

        if (normalized[0] != '/')
        {
            return false;
        }

        var hash = normalized.IndexOf('#');
        return hash < 0 || IsSectionId(normalized.Substring(hash + 1));
    }

    private static bool LooksLikePhone(string value)
    {
        var digits = 0;
        foreach (var ch in ToLatinDigits(value))
        {
            if (ch is >= '0' and <= '9')
            {
                digits++;
                continue;
            }

            if (ch is '+' or '-' or '(' or ')' || char.IsWhiteSpace(ch))
            {
                continue;
            }

            return false;
        }

        return digits >= 3;
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

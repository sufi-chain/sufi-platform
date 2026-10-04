using System.Globalization;
using Microsoft.Extensions.Localization;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Shared resource that stores menu labels. Seeded texts in other resources are still read.
/// </summary>
public static class MenuBusinessTexts
{
    public const string ResourceName = "SufiMenus";

    public const string DefaultCultureRequiredMessage = "عنوان فارسی الزامی است.";

    public const string LooksLikeKeyMessage = "این مقدار شبیه شناسهٔ سیستمی است؛ متن قابل نمایش وارد کنید.";

    public const string BrokenPlaceholderBanner = "عنوان این آیتم درست ذخیره نشده بود؛ لطفاً دوباره وارد کنید.";

    public const string CulturesFailedMessage = "فهرست زبان‌ها بارگیری نشد؛ فعلاً فقط فارسی.";

    public const string AdvancedSettingsLabel = "تنظیمات پیشرفته";

    public const string SameForAllLabel = "یک متن برای همه زبان‌ها";

    public const string KeyLabel = "شناسهٔ متن";

    public const string ScriptWarningMessage = "به نظر می‌رسد این متن به زبان دیگری است.";
}

/// <summary>
/// Plans menu-label keys, culture writes, and the public fallback chain without a database.
/// </summary>
public static class MenuDisplayNamePlanner
{
    public static string ItemKey(Guid menuId, Guid itemId) =>
        BusinessLocalizationKeys.SeededMenuItemDisplayName(menuId.ToString("N"), itemId.ToString("N"));

    public static string MenuKey(Guid menuId) =>
        BusinessLocalizationKeys.SeededMenuDisplayName(menuId.ToString("N"));

    /// <summary>
    /// Keeps an existing business key (including seeded keys). Placeholders and literals get <paramref name="generatedKey"/>.
    /// </summary>
    public static string ResolveStoredKey(string? storedDisplayName, string generatedKey)
    {
        if (BusinessLocalizationHelper.IsBusinessLocalizationKey(storedDisplayName)
            && !BusinessTextEditorStorage.IsPlaceholder(storedDisplayName))
        {
            return storedDisplayName!.Trim();
        }

        return generatedKey;
    }

    public static MenuDisplayNameWritePlan PlanWrite(
        IReadOnlyDictionary<string, string>? displayNames,
        string? defaultCulture,
        int maxLength)
    {
        var plan = new MenuDisplayNameWritePlan();
        if (displayNames == null)
        {
            plan.ErrorMessage = MenuBusinessTexts.DefaultCultureRequiredMessage;
            return plan;
        }

        var defaultPrimary = PrimaryLanguage(defaultCulture) ?? "fa";
        string? defaultValue = null;
        var sawDefault = false;

        foreach (var pair in displayNames)
        {
            var culture = SeedCultureHelper.NormalizeCulture(pair.Key);
            if (string.IsNullOrWhiteSpace(culture))
            {
                continue;
            }

            var normalized = NormalizeForCulture(culture, pair.Value);
            var isDefault = string.Equals(PrimaryLanguage(culture), defaultPrimary, StringComparison.OrdinalIgnoreCase);
            if (isDefault)
            {
                sawDefault = true;
                defaultValue = normalized;
            }

            if (IsBlankLabel(normalized))
            {
                if (!plan.Deletes.Contains(culture, StringComparer.OrdinalIgnoreCase))
                {
                    plan.Deletes.Add(culture);
                }

                plan.Upserts.Remove(culture);
                continue;
            }

            if (normalized.Length > maxLength)
            {
                plan.ErrorMessage ??= $"حداکثر {maxLength} نویسه.";
                continue;
            }

            if (LooksLikeSystemKey(normalized))
            {
                plan.ErrorMessage ??= MenuBusinessTexts.LooksLikeKeyMessage;
                continue;
            }

            plan.Deletes.RemoveAll(x => string.Equals(x, culture, StringComparison.OrdinalIgnoreCase));
            plan.Upserts[culture] = normalized;
        }

        if (!sawDefault || IsBlankLabel(defaultValue))
        {
            plan.ErrorMessage = MenuBusinessTexts.DefaultCultureRequiredMessage;
            plan.Upserts.Clear();
            plan.Deletes.Clear();
        }

        return plan;
    }

    public static MenuDisplayNameEditorState BuildEditorState(
        string? storedDisplayName,
        string? plainName,
        IEnumerable<string> cultures,
        string? defaultCulture,
        Func<string, string?> lookup)
    {
        var cultureList = cultures
            .Select(SeedCultureHelper.NormalizeCulture)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
        if (cultureList.Count == 0)
        {
            cultureList.Add("fa");
        }

        var values = cultureList.ToDictionary(x => x, _ => string.Empty, StringComparer.OrdinalIgnoreCase);
        var suggestion = IsBlankLabel(plainName) || LooksLikeSystemKey(plainName) ? null : plainName!.Trim();
        var state = new MenuDisplayNameEditorState
        {
            Values = values,
            Suggestion = suggestion
        };

        if (BusinessTextEditorStorage.IsPlaceholder(storedDisplayName))
        {
            state.BrokenPlaceholder = true;
            return state;
        }

        if (string.IsNullOrWhiteSpace(storedDisplayName))
        {
            return state;
        }

        var stored = storedDisplayName.Trim();
        if (!BusinessLocalizationHelper.IsBusinessLocalizationKey(stored))
        {
            if (!LooksLikeSystemKey(stored))
            {
                var culture = SeedCultureHelper.NormalizeCulture(defaultCulture) ?? "fa";
                if (!values.ContainsKey(culture))
                {
                    values[culture] = string.Empty;
                }

                values[culture] = NormalizeForCulture(culture, stored);
            }

            return state;
        }

        state.PreservedKey = stored;
        foreach (var culture in cultureList)
        {
            var raw = lookup(culture);
            values[culture] = IsDisplayableText(raw, stored) ? NormalizeForCulture(culture, raw!) : string.Empty;
        }

        return state;
    }

    /// <summary>
    /// Requested culture, then its parent (<c>fa-IR</c> → <c>fa</c>), then the default culture, then <paramref name="plainName"/>.
    /// A key, a placeholder, or a blank result never comes back.
    /// </summary>
    public static string ResolvePublicLabel(
        string? storedDisplayName,
        string? plainName,
        string? requestedCulture,
        string? defaultCulture,
        Func<string, string?> lookup)
    {
        var name = IsBlankLabel(plainName) || LooksLikeSystemKey(plainName) ? string.Empty : plainName!.Trim();
        if (string.IsNullOrWhiteSpace(storedDisplayName) || BusinessTextEditorStorage.IsPlaceholder(storedDisplayName))
        {
            return name;
        }

        var stored = storedDisplayName.Trim();
        if (!BusinessLocalizationHelper.IsBusinessLocalizationKey(stored))
        {
            return LooksLikeSystemKey(stored) ? name : stored;
        }

        foreach (var culture in CultureChain(requestedCulture, defaultCulture))
        {
            var raw = lookup(culture);
            if (IsDisplayableText(raw, stored))
            {
                return raw!.Trim();
            }
        }

        return name;
    }

    public static IReadOnlyList<string> CultureChain(string? requestedCulture, string? defaultCulture)
    {
        var chain = new List<string>();
        AddCulture(chain, requestedCulture);
        AddCulture(chain, ParentCulture(requestedCulture));
        AddCulture(chain, defaultCulture);
        AddCulture(chain, ParentCulture(defaultCulture));
        if (chain.Count == 0)
        {
            chain.Add("fa");
        }

        return chain;
    }

    public static string? LookupCulture(
        IStringLocalizerFactory stringLocalizerFactory,
        IEnumerable<string> resourceNames,
        string? key,
        string? culture)
    {
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        CultureInfo cultureInfo;
        try
        {
            cultureInfo = CultureInfo.GetCultureInfo(culture);
        }
        catch (CultureNotFoundException)
        {
            return null;
        }

        using (CultureHelper.Use(cultureInfo, cultureInfo))
        {
            foreach (var resourceName in resourceNames)
            {
                if (string.IsNullOrWhiteSpace(resourceName))
                {
                    continue;
                }

                var localizer = stringLocalizerFactory.CreateByResourceNameOrNull(resourceName);
                if (localizer == null)
                {
                    continue;
                }

                var localized = localizer[key];
                if (localized.ResourceNotFound || !IsDisplayableText(localized.Value, key))
                {
                    continue;
                }

                return localized.Value.Trim();
            }
        }

        return null;
    }

    public static bool IsBlankLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var withoutMarks = value.Trim().Replace("\u200c", string.Empty).Replace("\u200d", string.Empty);
        return string.IsNullOrWhiteSpace(withoutMarks);
    }

    public static bool LooksLikeSystemKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        if (trimmed.StartsWith('_'))
        {
            return true;
        }

        if (BusinessTextEditorStorage.IsPlaceholder(trimmed) || BusinessLocalizationHelper.IsBusinessLocalizationKey(trimmed))
        {
            return true;
        }

        return trimmed.Equals("DisplayName", StringComparison.Ordinal)
            || trimmed.Equals("ResourceName", StringComparison.Ordinal)
            || trimmed.Equals("LocalizationKey", StringComparison.Ordinal)
            || trimmed.Equals("LiteralValue", StringComparison.Ordinal)
            || trimmed.Equals("Values", StringComparison.Ordinal)
            || trimmed.Equals("_displayNames", StringComparison.Ordinal);
    }

    public static string NormalizeForCulture(string? culture, string? value)
    {
        var trimmed = value?.Trim() ?? string.Empty;
        if (!string.Equals(PrimaryLanguage(culture), "fa", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        return trimmed.Replace('\u064A', '\u06CC').Replace('\u0643', '\u06A9');
    }

    public static bool IsDisplayableText(string? value, string? storedKey)
    {
        if (IsBlankLabel(value) || LooksLikeSystemKey(value))
        {
            return false;
        }

        var trimmed = value!.Trim();
        return string.IsNullOrWhiteSpace(storedKey)
            || !string.Equals(trimmed, storedKey.Trim(), StringComparison.Ordinal);
    }

    public static string? PrimaryLanguage(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        var canonical = culture.Trim().Replace('_', '-');
        var dash = canonical.IndexOf('-');
        return (dash > 0 ? canonical[..dash] : canonical).ToLowerInvariant();
    }

    private static void AddCulture(List<string> chain, string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return;
        }

        var canonical = culture.Trim().Replace('_', '-');
        if (chain.Any(existing => string.Equals(existing, canonical, StringComparison.OrdinalIgnoreCase)))
        {
            return;
        }

        chain.Add(canonical);
    }

    private static string? ParentCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        var canonical = culture.Trim().Replace('_', '-');
        var dash = canonical.IndexOf('-');
        return dash > 0 ? canonical[..dash] : null;
    }
}

public sealed class MenuDisplayNameWritePlan
{
    public Dictionary<string, string> Upserts { get; } = new(StringComparer.OrdinalIgnoreCase);

    public List<string> Deletes { get; } = new();

    public string? ErrorMessage { get; set; }

    public bool Succeeded => string.IsNullOrEmpty(ErrorMessage);
}

public sealed class MenuDisplayNameEditorState
{
    public Dictionary<string, string> Values { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool BrokenPlaceholder { get; set; }

    public string? Suggestion { get; set; }

    public string? PreservedKey { get; set; }
}

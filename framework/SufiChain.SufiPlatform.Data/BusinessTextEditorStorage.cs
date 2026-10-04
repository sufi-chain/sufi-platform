namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Stores and renders business text from the literal and localization-key editor modes.
/// </summary>
public static class BusinessTextEditorStorage
{
    public const string UnboundLocalizationKey = "_localizationKey";

    public const string UnboundResourceName = "_localizationResourceName";

    public const string UnboundLiteralValue = "_literalDisplayName";

    public static bool IsPlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var trimmed = value.Trim();
        return string.Equals(trimmed, UnboundLocalizationKey, StringComparison.Ordinal)
            || string.Equals(trimmed, UnboundResourceName, StringComparison.Ordinal)
            || string.Equals(trimmed, UnboundLiteralValue, StringComparison.Ordinal);
    }

    public static bool HasLocalizationKey(string? localizationKey) =>
        !string.IsNullOrWhiteSpace(localizationKey) && !IsPlaceholder(localizationKey);

    /// <summary>Literal mode stores the typed label.</summary>
    public static string StoreLiteral(string? typedText) => typedText?.Trim() ?? string.Empty;

    /// <summary>Key mode stores the computed business key. An empty or placeholder key is rejected.</summary>
    public static string StoreLocalizationKey(string? computedKey)
    {
        if (!HasLocalizationKey(computedKey))
        {
            throw new ArgumentException("A non-empty localization key is required.", nameof(computedKey));
        }

        return computedKey!.Trim();
    }

    /// <summary>
    /// Renders a stored label. A business key uses <paramref name="localizedValue"/> when that
    /// value is a real translation. An empty, unresolved, or placeholder label uses <paramref name="plainName"/>.
    /// </summary>
    public static string ResolveDisplayName(string? storedDisplayName, string? plainName, string? localizedValue)
    {
        var fallback = string.IsNullOrWhiteSpace(plainName) ? string.Empty : plainName.Trim();
        if (string.IsNullOrWhiteSpace(storedDisplayName) || IsPlaceholder(storedDisplayName))
        {
            return fallback;
        }

        var stored = storedDisplayName.Trim();
        if (!BusinessLocalizationHelper.IsBusinessLocalizationKey(stored))
        {
            return stored;
        }

        if (string.IsNullOrWhiteSpace(localizedValue) || IsPlaceholder(localizedValue))
        {
            return fallback;
        }

        var localized = localizedValue.Trim();
        if (string.Equals(localized, stored, StringComparison.Ordinal))
        {
            return fallback;
        }

        return localized;
    }
}

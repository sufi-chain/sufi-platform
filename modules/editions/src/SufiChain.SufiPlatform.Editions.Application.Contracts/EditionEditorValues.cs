using System;

namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Pure edit-dialog rules. The stored display name may be a localization key; the field shows the
/// resolved label and writes the key back when that label was not changed.
/// </summary>
public static class EditionEditorValues
{
    public static string DisplayNameToStore(string? submitted, string? initialVisible, string? stored)
    {
        var visible = (submitted ?? string.Empty).Trim();
        var initial = (initialVisible ?? string.Empty).Trim();
        if (!string.IsNullOrEmpty(stored) && string.Equals(visible, initial, StringComparison.Ordinal))
        {
            return stored;
        }

        return visible;
    }

    public static bool ShouldPersistPrice(string? currencyCode) =>
        !string.IsNullOrWhiteSpace(currencyCode);

    public static bool PriceNeedsCurrency(decimal amount, string? currencyCode) =>
        amount > 0m && string.IsNullOrWhiteSpace(currencyCode);

    /// <summary>
    /// Label for the grid and the edit field. A stored <c>Plan:GROWTH:DisplayName</c> key
    /// becomes <c>Growth</c> when no translation was resolved.
    /// </summary>
    public static string VisibleLabel(string? localized, string? fallback)
    {
        var value = string.IsNullOrWhiteSpace(localized) ? fallback : localized;
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        if (!value.StartsWith("Plan:", StringComparison.Ordinal) || !value.EndsWith(":DisplayName", StringComparison.Ordinal))
        {
            return value;
        }

        var parts = value.Split(':');
        var token = parts.Length >= 2 ? parts[1].Trim() : value;
        if (token.Length == 0)
        {
            return value;
        }

        var lower = token.ToLowerInvariant();
        return char.ToUpperInvariant(lower[0]) + (lower.Length > 1 ? lower.Substring(1) : string.Empty);
    }
}

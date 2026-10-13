using System.Globalization;

namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Pure display helpers for the Editions grid price column (#193).
/// </summary>
public static class EditionPriceDisplay
{
    /// <summary>
    /// Returns a formatted amount, the localized "price not set" key marker, or null when
    /// no contributor attached a summary.
    /// </summary>
    public static string? Resolve(EditionPriceSummary? summary, string priceNotSetText)
    {
        if (summary is null)
        {
            return null;
        }

        if (summary.IsPriced && summary.Amount is { } amount)
        {
            var formatted = amount.ToString("N0", CultureInfo.InvariantCulture);
            var currency = summary.CurrencyCode ?? string.Empty;
            return string.IsNullOrWhiteSpace(currency)
                ? formatted
                : $"{formatted} {currency}";
        }

        return priceNotSetText;
    }
}

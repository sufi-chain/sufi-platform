using System.Globalization;
using SufiChain.SufiPlatform.Localization;

namespace SufiChain.SufiPlatform.Editions;

/// <summary>
/// Pure display helpers for the Editions grid price column (#193).
/// </summary>
public static class EditionPriceDisplay
{
    /// <summary>
    /// Returns the amount with two decimals and its currency, or the localized "price not set" text.
    /// An edition without a summary (no plan attached) also shows "price not set" so the cell is never empty.
    /// </summary>
    public static string Resolve(EditionPriceSummary? summary, string priceNotSetText, CultureInfo? culture = null)
    {
        if (summary is { IsPriced: true, Amount: { } amount })
        {
            return SufiMoneyFormat.Format(amount, summary.CurrencyCode, culture);
        }

        return priceNotSetText;
    }
}

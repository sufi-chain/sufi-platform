using System;
using System.Globalization;
using System.Text;

namespace SufiChain.SufiPlatform.Localization;

/// <summary>
/// Display formatting for money and credit amounts (sufi-chain#193, G-85/G-326/G-86).
/// Amounts are stored with up to 8-12 decimals; the UI shows exactly two, grouped, in the
/// current culture, and with Persian digits under <c>fa</c>. Display only: never use the result
/// for arithmetic or persistence.
/// </summary>
public static class SufiMoneyFormat
{
    public const int Decimals = 2;

    private const char PersianZero = '\u06F0';

    /// <summary>Formats with two decimals, e.g. <c>330,000.00</c> or <c>۳۳۰٬۰۰۰٫۰۰</c> under fa.</summary>
    public static string Format(decimal amount, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var rounded = Math.Round(amount, Decimals, MidpointRounding.AwayFromZero);
        return LocalizeDigits(rounded.ToString("N" + Decimals, culture), culture);
    }

    /// <summary>Formats with two decimals and appends the currency code when present.</summary>
    public static string Format(decimal amount, string? currencyCode, CultureInfo? culture = null)
    {
        var text = Format(amount, culture);
        return string.IsNullOrWhiteSpace(currencyCode) ? text : text + " " + currencyCode!.Trim();
    }

    /// <summary>Formats a whole number (no decimals), e.g. a Toman hint, with localized digits.</summary>
    public static string FormatWhole(decimal amount, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        var rounded = Math.Round(amount, 0, MidpointRounding.AwayFromZero);
        return LocalizeDigits(rounded.ToString("N0", culture), culture);
    }

    /// <summary>Replaces ASCII digits with Persian digits when the culture is Persian.</summary>
    public static string LocalizeDigits(string text, CultureInfo? culture = null)
    {
        culture ??= CultureInfo.CurrentCulture;
        if (string.IsNullOrEmpty(text) ||
            !string.Equals(culture.TwoLetterISOLanguageName, "fa", StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var ch in text)
        {
            builder.Append(ch >= '0' && ch <= '9' ? (char)(PersianZero + (ch - '0')) : ch);
        }

        return builder.ToString();
    }
}

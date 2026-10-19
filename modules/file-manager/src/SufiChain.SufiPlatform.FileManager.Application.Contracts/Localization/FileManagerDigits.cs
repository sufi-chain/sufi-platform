using System.Globalization;
using System.Text;

namespace SufiChain.SufiPlatform.FileManager.Localization;

/// <summary>
/// Shows numbers with Persian digits in the fa UI (sufi-chain#479: "100 MB" read as Latin digits in fa).
/// </summary>
public static class FileManagerDigits
{
    public static string Localize(string? text, CultureInfo? culture = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text ?? string.Empty;
        }

        culture ??= CultureInfo.CurrentUICulture;
        if (!string.Equals(culture.TwoLetterISOLanguageName, "fa", System.StringComparison.OrdinalIgnoreCase))
        {
            return text;
        }

        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
        {
            builder.Append(character is >= '0' and <= '9' ? (char)('\u06F0' + (character - '0')) : character);
        }

        return builder.ToString();
    }
}

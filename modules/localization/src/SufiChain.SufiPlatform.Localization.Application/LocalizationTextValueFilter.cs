using SufiChain.SufiPlatform.Localization.Dtos;

namespace SufiChain.SufiPlatform.Localization;

public static class LocalizationTextValueFilter
{
    public static bool Matches(LocalizationTextWithBaseValueDto item, string filter) =>
        Contains(item.Value, filter) || Contains(item.BaseValue, filter);

    public static bool MatchesSearch(LocalizationTextWithBaseValueDto item, string filter) =>
        Contains(item.Key, filter) || Matches(item, filter);

    public static bool Contains(string? text, string filter)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(filter))
        {
            return false;
        }

        return Fold(text).Contains(Fold(filter), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Drops U+200C. A و that the ZWNJ was separating (گفت‌وگو) is dropped with it,
    /// so the short form گفتگو matches.
    /// </summary>
    public static string Fold(string text)
    {
        var buffer = new System.Text.StringBuilder(text.Length);
        for (var i = 0; i < text.Length; i++)
        {
            if (text[i] != '\u200C')
            {
                buffer.Append(text[i]);
                continue;
            }

            if (i + 1 < text.Length && text[i + 1] == '\u0648')
            {
                i++;
            }
        }

        return buffer.ToString();
    }
}

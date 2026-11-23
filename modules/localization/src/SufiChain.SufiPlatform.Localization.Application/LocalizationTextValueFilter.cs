using SufiChain.SufiPlatform.Localization.Dtos;

namespace SufiChain.SufiPlatform.Localization;

public static class LocalizationTextValueFilter
{
    public static bool Matches(LocalizationTextWithBaseValueDto item, string filter) =>
        Contains(item.Value, filter) || Contains(item.BaseValue, filter);

    public static bool Contains(string? text, string filter) =>
        !string.IsNullOrEmpty(text) && text.Contains(filter, StringComparison.OrdinalIgnoreCase);
}

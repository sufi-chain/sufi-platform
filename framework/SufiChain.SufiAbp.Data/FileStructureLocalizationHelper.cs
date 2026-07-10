using Microsoft.Extensions.Localization;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Resolves file structure display names and descriptions from business-tier localization keys.
/// </summary>
public static class FileStructureLocalizationHelper
{
    public static bool IsBusinessLocalizationKey(string value)
    {
        return BusinessLocalizationHelper.IsBusinessLocalizationKey(value);
    }

    public static string ResolveText(
        IStringLocalizerFactory stringLocalizerFactory,
        string resourceName,
        string? storedKeyOrText,
        string fallback)
    {
        return BusinessLocalizationHelper.ResolveText(
            stringLocalizerFactory,
            resourceName,
            storedKeyOrText,
            fallback);
    }
}

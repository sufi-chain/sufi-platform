using System.Globalization;
using Microsoft.Extensions.Localization;
using Volo.Abp.Localization;

namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Resolves business-tier localization keys stored in entities and seeded content.
/// </summary>
public static class BusinessLocalizationHelper
{
    private static readonly string[] BusinessKeyPrefixes = new[]
    {
        "Structure:",
        "Copilot:",
        "MCPTool:",
        "MCPToolType:",
        "InboxCategory:",
        "SeededMenu:",
        "SeededCalendar:"
    };

    public static bool IsBusinessLocalizationKey(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        foreach (var prefix in BusinessKeyPrefixes)
        {
            if (value.StartsWith(prefix, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    public static string ResolveText(
        IStringLocalizerFactory stringLocalizerFactory,
        string? resourceName,
        string? storedKeyOrText,
        string fallback)
    {
        if (string.IsNullOrWhiteSpace(storedKeyOrText))
        {
            return fallback;
        }

        if (!IsBusinessLocalizationKey(storedKeyOrText))
        {
            return storedKeyOrText;
        }

        if (string.IsNullOrWhiteSpace(resourceName))
        {
            return fallback;
        }

        using (CultureHelper.Use(CultureInfo.CurrentUICulture))
        {
            var localizer = stringLocalizerFactory.CreateByResourceNameOrNull(resourceName);
            if (localizer == null)
            {
                return fallback;
            }

            var value = localizer[storedKeyOrText].Value;
            if (string.IsNullOrWhiteSpace(value) || string.Equals(value, storedKeyOrText, StringComparison.Ordinal))
            {
                return fallback;
            }

            return value.Trim();
        }
    }

    public static bool TryExtractSeededMenuKey(string displayNameKey, out string menuKey)
    {
        menuKey = string.Empty;

        if (!displayNameKey.StartsWith("SeededMenu:", StringComparison.Ordinal))
        {
            return false;
        }

        var parts = displayNameKey.Split(':');
        if (parts.Length < 3 || string.IsNullOrWhiteSpace(parts[1]))
        {
            return false;
        }

        menuKey = parts[1];
        return true;
    }
}

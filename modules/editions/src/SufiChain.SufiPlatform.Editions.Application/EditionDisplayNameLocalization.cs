using System;
using System.Collections.Generic;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Data;

namespace SufiChain.SufiPlatform.Editions;

internal static class EditionDisplayNameLocalization
{
    public static string Resolve(
        IStringLocalizerFactory stringLocalizerFactory,
        IEnumerable<string> resourceNames,
        string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return string.Empty;
        }

        var fallback = stored;
        foreach (var resourceName in resourceNames)
        {
            var resolved = BusinessLocalizationHelper.ResolveText(
                stringLocalizerFactory,
                resourceName,
                stored,
                fallback);

            if (!string.Equals(resolved, fallback, StringComparison.Ordinal))
            {
                return resolved;
            }
        }

        return Present(fallback);
    }

    /// <summary>
    /// A stored <c>Plan:GROWTH:DisplayName</c> key is not a label. When no resource has
    /// text for it, show the plan token (<c>Growth</c>) instead of the raw key.
    /// </summary>
    public static string Present(string? value) => EditionEditorValues.VisibleLabel(value, value);
}

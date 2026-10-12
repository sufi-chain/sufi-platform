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

        return fallback;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;

namespace SufiChain.SufiAbp.Data;

/// <summary>
/// Resolves seed cultures from <see cref="DataSeedContext"/> properties with <see cref="SufiAbpDataSeedOptions"/> fallback.
/// </summary>
public static class SeedCultureHelper
{
    public static string GetDefaultCulture(DataSeedContext context, SufiAbpDataSeedOptions options)
    {
        if (context.Properties.TryGetValue(SufiAbpConstants.DefaultCulturePropertyName, out var culture)
            && culture is string cultureName
            && !string.IsNullOrWhiteSpace(cultureName))
        {
            return NormalizeCulture(cultureName)!;
        }

        return NormalizeCulture(options.DefaultCulture) ?? "fa";
    }

    public static string GetDefaultCulture(DataSeedContext context, IOptions<SufiAbpDataSeedOptions> options)
    {
        return GetDefaultCulture(context, options.Value);
    }

    public static IReadOnlyList<string> GetSupportedCultures(DataSeedContext context, SufiAbpDataSeedOptions options)
    {
        if (context.Properties.TryGetValue(SufiAbpConstants.SupportedCulturesPropertyName, out var cultures)
            && cultures is string[] cultureArray
            && cultureArray.Length > 0)
        {
            return cultureArray
                .Select(NormalizeCulture)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Cast<string>()
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        return options.SupportedCultures
            .Select(NormalizeCulture)
            .Where(c => !string.IsNullOrWhiteSpace(c))
            .Cast<string>()
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public static IReadOnlyList<string> GetSupportedCultures(DataSeedContext context, IOptions<SufiAbpDataSeedOptions> options)
    {
        return GetSupportedCultures(context, options.Value);
    }

    public static string? NormalizeCulture(string? culture)
    {
        if (string.IsNullOrWhiteSpace(culture))
        {
            return null;
        }

        return culture.Trim().Split('-', '_')[0].ToLowerInvariant();
    }
}

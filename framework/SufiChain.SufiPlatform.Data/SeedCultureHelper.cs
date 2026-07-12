using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Data;

/// <summary>
/// Resolves seed cultures from <see cref="DataSeedContext"/> properties with <see cref="SufiDataSeedOptions"/> fallback.
/// </summary>
public static class SeedCultureHelper
{
    public static string GetDefaultCulture(DataSeedContext context, SufiDataSeedOptions options)
    {
        if (context.Properties.TryGetValue(SufiConstants.DefaultCulturePropertyName, out var culture)
            && culture is string cultureName
            && !string.IsNullOrWhiteSpace(cultureName))
        {
            return NormalizeCulture(cultureName)!;
        }

        return NormalizeCulture(options.DefaultCulture) ?? "fa";
    }

    public static string GetDefaultCulture(DataSeedContext context, IOptions<SufiDataSeedOptions> options)
    {
        return GetDefaultCulture(context, options.Value);
    }

    public static IReadOnlyList<string> GetSupportedCultures(DataSeedContext context, SufiDataSeedOptions options)
    {
        if (context.Properties.TryGetValue(SufiConstants.SupportedCulturesPropertyName, out var cultures)
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

    public static IReadOnlyList<string> GetSupportedCultures(DataSeedContext context, IOptions<SufiDataSeedOptions> options)
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

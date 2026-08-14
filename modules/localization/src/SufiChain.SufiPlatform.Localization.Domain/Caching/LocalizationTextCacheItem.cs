using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.Localization.Caching;

/// <summary>
/// Cache item that holds all localization texts for a specific resource + culture combination.
/// Cached as a dictionary of key -> value for fast lookups.
/// </summary>
[Serializable]
public class LocalizationTextCacheItem
{
    /// <summary>
    /// Dictionary of localization key -> translated value.
    /// </summary>
    public Dictionary<string, string> Texts { get; set; } = new();

    public static string CalculateCacheKey(Guid? tenantId, string resourceName, string cultureName)
    {
        var tenantSegment = tenantId.HasValue
            ? tenantId.Value.ToString("N")
            : "host";
        return $"t:{tenantSegment},r:{resourceName},c:{cultureName}";
    }
}

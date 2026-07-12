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

    public static string CalculateCacheKey(string resourceName, string cultureName)
    {
        return $"r:{resourceName},c:{cultureName}";
    }
}

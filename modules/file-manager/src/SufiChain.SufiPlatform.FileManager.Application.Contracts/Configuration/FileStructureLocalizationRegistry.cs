using System;
using System.Collections.Generic;

namespace SufiChain.SufiPlatform.FileManager.Configuration;

/// <summary>
/// Maps file structure keys to the owning module localization resource used for business-tier display text.
/// </summary>
public static class FileStructureLocalizationRegistry
{
    private static readonly Dictionary<string, string> ResourceNames =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public const string DefaultResourceName = "SufiFileManager";

    public static void Register(string structureKey, string localizationResourceName)
    {
        if (string.IsNullOrWhiteSpace(structureKey))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(structureKey));
        }

        if (string.IsNullOrWhiteSpace(localizationResourceName))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(localizationResourceName));
        }

        ResourceNames[structureKey] = localizationResourceName;
    }

    public static string GetResourceName(string structureKey, string? fallback = DefaultResourceName)
    {
        return ResourceNames.TryGetValue(structureKey, out var resourceName)
            ? resourceName
            : fallback ?? DefaultResourceName;
    }

    public static IReadOnlyDictionary<string, string> GetAll() => ResourceNames;
}
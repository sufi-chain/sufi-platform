using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.FileManager.Blazor;

/// <summary>
/// Base class for Blazor components in the File Manager module.
/// Provides module localization via FileManagerResource.
/// </summary>
public abstract class FileManagerComponentBase : SufiComponentBase
{
    protected FileManagerComponentBase()
    {
        LocalizationResource = typeof(SufiFileManagerResource);
    }

    protected string ResolveBusinessText(string resourceName, string? keyOrText, string fallback = "")
    {
        return FileStructureLocalizationHelper.ResolveText(
            StringLocalizerFactory,
            resourceName,
            keyOrText,
            string.IsNullOrWhiteSpace(fallback) ? keyOrText ?? string.Empty : fallback);
    }

    protected string ResolveStructureText(FileStructureDto structure)
    {
        return ResolveStructureText(
            structure.Key,
            structure.DisplayName,
            structure.LocalizationResourceName);
    }

    protected string ResolveStructureText(
        string structureKey,
        string? displayNameKey,
        string? localizationResourceName = null)
    {
        var resourceName = localizationResourceName
            ?? Configuration.FileStructureLocalizationRegistry.GetResourceName(structureKey);
        var key = string.IsNullOrWhiteSpace(displayNameKey)
            ? BusinessLocalizationKeys.FileStructureDisplayName(structureKey)
            : displayNameKey;

        return ResolveBusinessText(resourceName, key, structureKey);
    }

    protected string ResolveStructureDisplayName(string structureKey, string? storedKeyOrText = null)
    {
        return ResolveStructureText(
            structureKey,
            storedKeyOrText ?? BusinessLocalizationKeys.FileStructureDisplayName(structureKey),
            null);
    }

    protected string ResolveStructureDescription(string structureKey, string? storedDescriptionKey)
    {
        if (string.IsNullOrWhiteSpace(storedDescriptionKey))
        {
            return string.Empty;
        }

        var resourceName = FileStructureLocalizationRegistry.GetResourceName(structureKey);
        return ResolveBusinessText(resourceName, storedDescriptionKey, storedDescriptionKey);
    }

    protected string ResolveStructureDescription(FileStructureDto structure)
    {
        if (string.IsNullOrWhiteSpace(structure.Description))
        {
            return string.Empty;
        }

        var resourceName = structure.LocalizationResourceName
            ?? Configuration.FileStructureLocalizationRegistry.GetResourceName(structure.Key);

        return ResolveBusinessText(
            resourceName,
            structure.Description,
            structure.Description);
    }

    protected string ResolveStructureText(FileStructureDefaultDto config)
    {
        return ResolveStructureText(
            config.Key,
            config.DisplayName,
            config.LocalizationResourceName);
    }

    protected string ResolveStructureDescription(FileStructureDefaultDto config)
    {
        if (string.IsNullOrWhiteSpace(config.Description))
        {
            return string.Empty;
        }

        var resourceName = config.LocalizationResourceName
            ?? Configuration.FileStructureLocalizationRegistry.GetResourceName(config.Key);

        return ResolveBusinessText(
            resourceName,
            config.Description,
            config.Description);
    }
}
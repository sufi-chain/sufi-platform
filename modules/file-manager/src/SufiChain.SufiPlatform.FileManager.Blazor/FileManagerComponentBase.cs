using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.FileManager.Configuration;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
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

    protected async Task NotifyOperationFailedAsync(Exception exception, string localizationKey)
    {
        Logger.LogError(exception, "File Manager UI operation failed: {LocalizationKey}", localizationKey);
        await Notify.ErrorAsync(L[localizationKey]);
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

    protected string ResolveFolderDisplayName(
        string? structureKey,
        string? name,
        bool isStructureFolder,
        string? localizationResourceName = null)
    {
        if (isStructureFolder && !string.IsNullOrWhiteSpace(structureKey))
        {
            return ResolveStructureDisplayName(structureKey, name);
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            return string.Empty;
        }

        if (!string.IsNullOrWhiteSpace(localizationResourceName) &&
            FileStructureLocalizationHelper.IsBusinessLocalizationKey(name))
        {
            return ResolveBusinessText(localizationResourceName, name, name);
        }

        if (!string.IsNullOrWhiteSpace(structureKey) &&
            FileStructureLocalizationHelper.IsBusinessLocalizationKey(name))
        {
            var resourceName = FileStructureLocalizationRegistry.GetResourceName(structureKey);
            return ResolveBusinessText(resourceName, name, name);
        }

        return name;
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

    protected string FormatFileType(FileType fileType) => fileType switch
    {
        FileType.Image => L["FileTypeImage"],
        FileType.Video => L["FileTypeVideo"],
        FileType.Audio => L["FileTypeAudio"],
        FileType.Document => L["FileTypeDocument"],
        _ => L["FileItem"]
    };

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
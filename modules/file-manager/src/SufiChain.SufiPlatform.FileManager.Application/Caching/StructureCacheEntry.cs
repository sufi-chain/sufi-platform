using System.Collections.Generic;
using SufiChain.SufiPlatform.FileManager.FileTypes;

namespace SufiChain.SufiPlatform.FileManager.Caching;

/// <summary>
/// Cached snapshot of a FileStructure for access by consumers without hitting the database.
/// Includes ExtraProperties for blob storage configuration resolution.
/// </summary>
public class StructureCacheEntry
{
    public string Key { get; set; } = default!;
    public bool IsPublicAccess { get; set; }
    public string? BaseUrl { get; set; }
    public long MaxFileSize { get; set; }
    public string AllowedExtensions { get; set; } = default!;
    public string AllowedMimeTypes { get; set; } = default!;
    public bool GenerateThumbnail { get; set; }
    public int ThumbnailWidth { get; set; } = 200;
    public int ThumbnailHeight { get; set; } = 200;
    public bool EnableWebPConversion { get; set; }
    public int WebPQuality { get; set; } = 80;
    public FileType AllowedFileTypes { get; set; }
    public int? MinImageWidth { get; set; }
    public int? MinImageHeight { get; set; }
    public int? MaxImageWidth { get; set; }
    public int? MaxImageHeight { get; set; }

    /// <summary>
    /// Storage config from ExtraProperties (encrypted values for secrets).
    /// Used by StructureBlobContainerConfigurationProvider to build blob config.
    /// </summary>
    public Dictionary<string, object>? ExtraProperties { get; set; }
}

using SufiChain.SufiPlatform.FileManager.FileTypes;

namespace SufiChain.SufiPlatform.FileManager.Configuration;

/// <summary>
/// Configuration for a file structure
/// </summary>
public class FileStructureConfig
{
    public string Key { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }

    /// <summary>
    /// Owning module localization resource for <see cref="DisplayName"/> and <see cref="Description"/> keys.
    /// </summary>
    public string LocalizationResourceName { get; set; } = FileStructureLocalizationRegistry.DefaultResourceName;

    public FileType AllowedFileTypes { get; set; }
    public string AllowedExtensions { get; set; } = default!;
    public string AllowedMimeTypes { get; set; } = default!;
    public long MaxFileSize { get; set; }
    public int? MinImageWidth { get; set; }
    public int? MinImageHeight { get; set; }
    public int? MaxImageWidth { get; set; }
    public int? MaxImageHeight { get; set; }
    public bool IsMultiple { get; set; }
    public int? MaxCount { get; set; }
    public bool IsRequired { get; set; }
    public bool GenerateThumbnail { get; set; }
    public int ThumbnailWidth { get; set; } = 200;
    public int ThumbnailHeight { get; set; } = 200;
    public bool EnableWebPConversion { get; set; }
    public int WebPQuality { get; set; } = 80;
    public string? StorageProvider { get; set; }
    public bool IsPublicAccess { get; set; }
    public string? BaseUrl { get; set; }
    public bool ResizeLargeImages { get; set; }

    /// <summary>
    /// When true, the structure is seeded by the platform and cannot be deleted by users.
    /// </summary>
    public bool IsStatic { get; set; }
}

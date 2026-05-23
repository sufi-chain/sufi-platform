using System;
using SufiChain.SufiAbp.FileManager.FileTypes;
using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiAbp.FileManager.FileStructures;

/// <summary>
/// Defines validation rules and processing options for file uploads
/// </summary>
public class FileStructure : AuditedAggregateRoot<Guid>
{
    /// <summary>
    /// Unique key identifier (e.g., "Product.MainImage", "Article.Gallery")
    /// </summary>
    public string Key { get; set; } = default!;
    
    /// <summary>
    /// Display name for UI
    /// </summary>
    public string DisplayName { get; set; } = default!;
    
    /// <summary>
    /// Description of the structure's purpose
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Allowed file types (flags enum)
    /// </summary>
    public FileType AllowedFileTypes { get; set; }
    
    /// <summary>
    /// Allowed file extensions (comma-separated, e.g., "jpg,png,webp")
    /// </summary>
    public string AllowedExtensions { get; set; } = default!;
    
    /// <summary>
    /// Allowed MIME types (comma-separated)
    /// </summary>
    public string AllowedMimeTypes { get; set; } = default!;
    
    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public long MaxFileSize { get; set; }
    
    /// <summary>
    /// Minimum image width in pixels
    /// </summary>
    public int? MinImageWidth { get; set; }
    
    /// <summary>
    /// Minimum image height in pixels
    /// </summary>
    public int? MinImageHeight { get; set; }
    
    /// <summary>
    /// Maximum image width in pixels
    /// </summary>
    public int? MaxImageWidth { get; set; }
    
    /// <summary>
    /// Maximum image height in pixels
    /// </summary>
    public int? MaxImageHeight { get; set; }
    
    /// <summary>
    /// Whether multiple files are allowed
    /// </summary>
    public bool IsMultiple { get; set; }
    
    /// <summary>
    /// Maximum number of files (if IsMultiple is true)
    /// </summary>
    public int? MaxCount { get; set; }
    
    /// <summary>
    /// Whether at least one file is required
    /// </summary>
    public bool IsRequired { get; set; }
    
    /// <summary>
    /// Whether to generate thumbnails
    /// </summary>
    public bool GenerateThumbnail { get; set; }
    
    /// <summary>
    /// Thumbnail width in pixels
    /// </summary>
    public int ThumbnailWidth { get; set; } = 200;
    
    /// <summary>
    /// Thumbnail height in pixels
    /// </summary>
    public int ThumbnailHeight { get; set; } = 200;
    
    /// <summary>
    /// Whether to convert images to WebP format
    /// </summary>
    public bool EnableWebPConversion { get; set; }
    
    /// <summary>
    /// WebP quality (1-100)
    /// </summary>
    public int WebPQuality { get; set; } = 80;
    
    /// <summary>
    /// Storage provider name (null for default)
    /// </summary>
    public string? StorageProvider { get; set; }
    
    /// <summary>
    /// Whether files should be publicly accessible
    /// </summary>
    public bool IsPublicAccess { get; set; }

    /// <summary>
    /// Base URL for file links (download/thumbnail/stream). When null, falls back to
    /// FileManagerOptions.BaseUrl (API) or RemoteServices:FileManager:BaseUrl (Blazor).
    /// </summary>
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Whether to resize large images automatically
    /// </summary>
    public bool ResizeLargeImages { get; set; }

    protected FileStructure()
    {
    }

    public FileStructure(
        Guid id,
        string key,
        string displayName,
        FileType allowedFileTypes,
        string allowedExtensions,
        string allowedMimeTypes,
        long maxFileSize) : base(id)
    {
        Key = key;
        DisplayName = displayName;
        AllowedFileTypes = allowedFileTypes;
        AllowedExtensions = allowedExtensions;
        AllowedMimeTypes = allowedMimeTypes;
        MaxFileSize = maxFileSize;
    }

    public void SetImageConstraints(
        int? minWidth = null,
        int? minHeight = null,
        int? maxWidth = null,
        int? maxHeight = null)
    {
        MinImageWidth = minWidth;
        MinImageHeight = minHeight;
        MaxImageWidth = maxWidth;
        MaxImageHeight = maxHeight;
    }

    public void ConfigureThumbnail(bool generate, int width = 200, int height = 200)
    {
        GenerateThumbnail = generate;
        ThumbnailWidth = width;
        ThumbnailHeight = height;
    }

    public void ConfigureWebPConversion(bool enable, int quality = 80)
    {
        EnableWebPConversion = enable;
        WebPQuality = Math.Clamp(quality, 1, 100);
    }

    public void SetMultipleFilesConfig(bool isMultiple, int? maxCount = null)
    {
        IsMultiple = isMultiple;
        MaxCount = maxCount;
    }
}

using System;
using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Storage;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.FileManager.FileStructures;

public class FileStructureDto : AuditedEntityDto<Guid>
{
    public string Key { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }

    /// <summary>
    /// Owning module localization resource for business-tier display keys.
    /// </summary>
    public string LocalizationResourceName { get; set; } = Configuration.FileStructureLocalizationRegistry.DefaultResourceName;

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
    public int ThumbnailWidth { get; set; }
    public int ThumbnailHeight { get; set; }
    public bool EnableWebPConversion { get; set; }
    public int WebPQuality { get; set; }
    public bool ResizeLargeImages { get; set; }
    public string? StorageProvider { get; set; }
    public bool IsPublicAccess { get; set; }
    public string? BaseUrl { get; set; }
    
    /// <summary>
    /// Indicates if this structure is platform-defined and cannot be deleted.
    /// </summary>
    public bool IsStatic { get; set; }

    /// <summary>
    /// Indicates if this structure has a developer-defined default configuration
    /// </summary>
    public bool HasDefaultConfig { get; set; }
    
    /// <summary>
    /// Indicates if the current configuration differs from the default
    /// </summary>
    public bool IsModifiedFromDefault { get; set; }

    /// <summary>
    /// Storage configuration (read-only; sensitive values are not exposed)
    /// </summary>
    public FileStructureStorageConfigDto? StorageConfig { get; set; }
}

public class CreateUpdateFileStructureDto
{
    [Required]
    [StringLength(256)]
    [RegularExpression(@"^[A-Za-z0-9][\w.]*$", ErrorMessage = "FileManager:KeyFormatInvalid")]
    public string Key { get; set; } = default!;

    [Required]
    [StringLength(256)]
    public string DisplayName { get; set; } = default!;

    [StringLength(1024)]
    public string? Description { get; set; }

    public FileType AllowedFileTypes { get; set; }

    [Required]
    [StringLength(512)]
    public string AllowedExtensions { get; set; } = default!;

    [Required]
    [StringLength(1024)]
    public string AllowedMimeTypes { get; set; } = default!;

    [Range(1, long.MaxValue)]
    public long MaxFileSize { get; set; }
    public int? MinImageWidth { get; set; }
    public int? MinImageHeight { get; set; }
    public int? MaxImageWidth { get; set; }
    public int? MaxImageHeight { get; set; }
    public bool IsMultiple { get; set; }
    public int? MaxCount { get; set; }
    public bool IsRequired { get; set; }
    public bool GenerateThumbnail { get; set; }
    public int ThumbnailWidth { get; set; }
    public int ThumbnailHeight { get; set; }
    public bool EnableWebPConversion { get; set; }
    public int WebPQuality { get; set; } = 80;
    public bool ResizeLargeImages { get; set; }
    public string? StorageProvider { get; set; }
    public bool IsPublicAccess { get; set; }
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Storage configuration for create/update
    /// </summary>
    public FileStructureStorageConfigDto? StorageConfig { get; set; }
}

/// <summary>
/// Represents the default (developer-defined) configuration for a file structure
/// </summary>
public class FileStructureDefaultDto
{
    public string Key { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Description { get; set; }
    public string LocalizationResourceName { get; set; } = Configuration.FileStructureLocalizationRegistry.DefaultResourceName;
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
    public int ThumbnailWidth { get; set; }
    public int ThumbnailHeight { get; set; }
    public bool EnableWebPConversion { get; set; }
    public int WebPQuality { get; set; }
    public bool ResizeLargeImages { get; set; }
    public string? StorageProvider { get; set; }
    public bool IsPublicAccess { get; set; }
    public string? BaseUrl { get; set; }
    public bool IsStatic { get; set; }
}

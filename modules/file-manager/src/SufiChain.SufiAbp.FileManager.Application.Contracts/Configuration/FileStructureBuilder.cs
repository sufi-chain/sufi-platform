using System;
using System.Linq;
using SufiChain.SufiAbp.FileManager.FileTypes;

namespace SufiChain.SufiAbp.FileManager.Configuration;

/// <summary>
/// Fluent API builder for configuring file structures
/// </summary>
public class FileStructureBuilder
{
    private readonly FileManagerOptions _options;
    private readonly FileStructureConfig _config;

    public FileStructureBuilder(FileManagerOptions options, string key)
    {
        _options = options;
        _config = new FileStructureConfig
        {
            Key = key,
            DisplayName = key,
            AllowedFileTypes = FileType.None,
            MaxFileSize = 10 * 1024 * 1024, // 10MB default
            EnableWebPConversion = options.EnableWebPConversionByDefault,
            ResizeLargeImages = options.ResizeLargeImagesByDefault,
            WebPQuality = options.DefaultWebPQuality
        };
        _options.Structures.Add(_config);
    }

    public FileStructureBuilder WithDisplayName(string displayName)
    {
        _config.DisplayName = displayName;
        return this;
    }

    public FileStructureBuilder WithDescription(string description)
    {
        _config.Description = description;
        return this;
    }

    public FileStructureBuilder ForFileTypes(FileType fileTypes)
    {
        _config.AllowedFileTypes = fileTypes;
        UpdateDefaultExtensionsAndMimeTypes();
        return this;
    }

    public FileStructureBuilder ForImages()
    {
        return ForFileTypes(FileType.Image);
    }

    public FileStructureBuilder ForVideos()
    {
        return ForFileTypes(FileType.Video);
    }

    public FileStructureBuilder ForDocuments()
    {
        return ForFileTypes(FileType.Document);
    }

    public FileStructureBuilder AllowExtensions(params string[] extensions)
    {
        _config.AllowedExtensions = string.Join(",", extensions);
        return this;
    }

    public FileStructureBuilder AllowMimeTypes(params string[] mimeTypes)
    {
        _config.AllowedMimeTypes = string.Join(",", mimeTypes);
        return this;
    }

    public FileStructureBuilder AlsoAllowMimeTypes(params string[] mimeTypes)
    {
        var existing = (_config.AllowedMimeTypes ?? "")
            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !string.IsNullOrEmpty(x))
            .ToList();
        existing.AddRange(mimeTypes);
        _config.AllowedMimeTypes = string.Join(",", existing.Distinct(StringComparer.OrdinalIgnoreCase));
        return this;
    }

    public FileStructureBuilder WithMaxSize(long sizeInBytes)
    {
        _config.MaxFileSize = sizeInBytes;
        return this;
    }

    public FileStructureBuilder WithImageDimensions(
        int? minWidth = null,
        int? minHeight = null,
        int? maxWidth = null,
        int? maxHeight = null)
    {
        _config.MinImageWidth = minWidth;
        _config.MinImageHeight = minHeight;
        _config.MaxImageWidth = maxWidth;
        _config.MaxImageHeight = maxHeight;
        return this;
    }

    public FileStructureBuilder SingleFile()
    {
        _config.IsMultiple = false;
        _config.MaxCount = 1;
        return this;
    }

    public FileStructureBuilder MultipleFiles(int? maxCount = null)
    {
        _config.IsMultiple = true;
        _config.MaxCount = maxCount;
        return this;
    }

    public FileStructureBuilder Required(bool isRequired = true)
    {
        _config.IsRequired = isRequired;
        return this;
    }

    public FileStructureBuilder GenerateThumbnail(bool generate = true, int width = 200, int height = 200)
    {
        _config.GenerateThumbnail = generate;
        _config.ThumbnailWidth = width;
        _config.ThumbnailHeight = height;
        return this;
    }

    public FileStructureBuilder EnableWebPConversion(bool enable = true, int quality = 80)
    {
        _config.EnableWebPConversion = enable;
        _config.WebPQuality = quality;
        return this;
    }

    public FileStructureBuilder StoreInProvider(string? providerName)
    {
        _config.StorageProvider = providerName;
        return this;
    }

    public FileStructureBuilder IsPublic(bool isPublic = true)
    {
        _config.IsPublicAccess = isPublic;
        return this;
    }

    public FileStructureBuilder WithBaseUrl(string? baseUrl)
    {
        _config.BaseUrl = baseUrl;
        return this;
    }

    public FileStructureBuilder ResizeLargeImages(bool resize = true)
    {
        _config.ResizeLargeImages = resize;
        return this;
    }

    private void UpdateDefaultExtensionsAndMimeTypes()
    {
        var extensions = new System.Collections.Generic.List<string>();
        var mimeTypes = new System.Collections.Generic.List<string>();

        if (_config.AllowedFileTypes.HasFlag(FileType.Image))
        {
            extensions.AddRange(new[] { "jpg", "jpeg", "png", "gif", "webp", "bmp", "svg" });
            mimeTypes.AddRange(new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/bmp", "image/svg+xml" });
        }

        if (_config.AllowedFileTypes.HasFlag(FileType.Video))
        {
            extensions.AddRange(new[] { "mp4", "webm", "mov", "avi", "mkv" });
            mimeTypes.AddRange(new[] { "video/mp4", "video/webm", "video/quicktime", "video/x-msvideo", "video/x-matroska" });
        }

        if (_config.AllowedFileTypes.HasFlag(FileType.Document))
        {
            extensions.AddRange(new[] { "pdf", "doc", "docx", "xls", "xlsx", "ppt", "pptx", "txt" });
            mimeTypes.AddRange(new[] { "application/pdf", "application/msword", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/vnd.ms-excel", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
        }

        if (_config.AllowedFileTypes.HasFlag(FileType.Audio))
        {
            extensions.AddRange(new[] { "mp3", "wav", "ogg", "flac" });
            mimeTypes.AddRange(new[] { "audio/mpeg", "audio/wav", "audio/ogg", "audio/flac" });
        }

        _config.AllowedExtensions = string.Join(",", extensions.Distinct());
        _config.AllowedMimeTypes = string.Join(",", mimeTypes.Distinct());
    }
}

/// <summary>
/// Extension methods for size configuration
/// </summary>
public static class SizeExtensions
{
    public static long KB(this int value) => value * 1024L;
    public static long KB(this long value) => value * 1024L;
    public static long MB(this int value) => value * 1024L * 1024L;
    public static long MB(this long value) => value * 1024L * 1024L;
    public static long GB(this int value) => value * 1024L * 1024L * 1024L;
    public static long GB(this long value) => value * 1024L * 1024L * 1024L;
}

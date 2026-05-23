namespace SufiChain.SufiAbp.FileManager.Blazor.Public.Services;

/// <summary>
/// Resolves public URLs for file items.
/// Used by public-facing components to generate correct URLs for images, downloads, etc.
/// </summary>
public interface IFilePublicUrlResolver
{
    /// <summary>
    /// Gets the public download URL for a file.
    /// </summary>
    Task<string?> GetDownloadUrlAsync(Guid fileId);

    /// <summary>
    /// Gets the public thumbnail URL for an image.
    /// </summary>
    Task<string?> GetThumbnailUrlAsync(Guid fileId, FileImageSize? size = null);

    /// <summary>
    /// Gets the public stream URL for video/audio files.
    /// </summary>
    Task<string?> GetStreamUrlAsync(Guid fileId);

    /// <summary>
    /// Gets all size variant URLs for an image (for srcset).
    /// </summary>
    Task<Dictionary<FileImageSize, string>> GetImageVariantsAsync(Guid fileId);

    /// <summary>
    /// Resolves file info with URLs for public display.
    /// </summary>
    Task<FilePublicInfo?> GetFilePublicInfoAsync(Guid fileId);
}

/// <summary>
/// Image size variants for responsive images.
/// </summary>
public enum FileImageSize
{
    /// <summary>
    /// Thumbnail size (typically 150px)
    /// </summary>
    Thumbnail = 150,

    /// <summary>
    /// Small size (typically 320px)
    /// </summary>
    Small = 320,

    /// <summary>
    /// Medium size (typically 640px)
    /// </summary>
    Medium = 640,

    /// <summary>
    /// Large size (typically 1024px)
    /// </summary>
    Large = 1024,

    /// <summary>
    /// Original size (no resizing)
    /// </summary>
    Original = 0
}

/// <summary>
/// Public file information for display components.
/// </summary>
public class FilePublicInfo
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public long Size { get; set; }
    public string? DownloadUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public string? StreamUrl { get; set; }
    public Dictionary<FileImageSize, string> SizeVariants { get; set; } = new();
    public bool IsImage { get; set; }
    public bool IsVideo { get; set; }
    public bool IsAudio { get; set; }
}

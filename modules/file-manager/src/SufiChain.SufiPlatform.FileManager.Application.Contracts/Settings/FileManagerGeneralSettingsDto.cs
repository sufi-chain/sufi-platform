namespace SufiChain.SufiPlatform.FileManager.Settings;

/// <summary>
/// Tenant-level general File Manager settings.
/// </summary>
public class FileManagerGeneralSettingsDto
{
    /// <summary>
    /// Storage quota per tenant in MB (0 = unlimited).
    /// </summary>
    public long StorageQuotaMB { get; set; }

    /// <summary>
    /// Maximum file size in bytes.
    /// </summary>
    public long MaxFileSizeBytes { get; set; }

    public string AllowedImageExtensions { get; set; } = string.Empty;

    public string AllowedVideoExtensions { get; set; } = string.Empty;

    public string AllowedDocumentExtensions { get; set; } = string.Empty;

    public bool EnableWebPConversion { get; set; }

    public int WebPQuality { get; set; }

    public int ThumbnailWidth { get; set; }

    public int ThumbnailHeight { get; set; }

    public int MaxImageWidth { get; set; }

    public int MaxImageHeight { get; set; }

    public int AutoDeleteTempMediaAfterDays { get; set; }

    public bool EnableDuplicateDetection { get; set; }
}

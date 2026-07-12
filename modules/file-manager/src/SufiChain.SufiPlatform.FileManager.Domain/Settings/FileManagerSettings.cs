namespace SufiChain.SufiPlatform.FileManager.Settings;

public static class FileManagerSettings
{
    public const string GroupName = "SufiFileManager";

    /// <summary>
    /// Storage quota per tenant in MB (0 = unlimited)
    /// </summary>
    public const string StorageQuota = GroupName + ".StorageQuota";

    /// <summary>
    /// Maximum file size in bytes
    /// </summary>
    public const string MaxFileSize = GroupName + ".MaxFileSize";

    /// <summary>
    /// Comma-separated list of allowed image extensions
    /// </summary>
    public const string AllowedImageExtensions = GroupName + ".AllowedImageExtensions";

    /// <summary>
    /// Comma-separated list of allowed video extensions
    /// </summary>
    public const string AllowedVideoExtensions = GroupName + ".AllowedVideoExtensions";

    /// <summary>
    /// Comma-separated list of allowed document extensions
    /// </summary>
    public const string AllowedDocumentExtensions = GroupName + ".AllowedDocumentExtensions";

    /// <summary>
    /// Whether to enable WebP conversion
    /// </summary>
    public const string EnableWebPConversion = GroupName + ".EnableWebPConversion";

    /// <summary>
    /// WebP quality (1-100)
    /// </summary>
    public const string WebPQuality = GroupName + ".WebPQuality";

    /// <summary>
    /// Default thumbnail width
    /// </summary>
    public const string ThumbnailWidth = GroupName + ".ThumbnailWidth";

    /// <summary>
    /// Default thumbnail height
    /// </summary>
    public const string ThumbnailHeight = GroupName + ".ThumbnailHeight";

    /// <summary>
    /// Maximum image width
    /// </summary>
    public const string MaxImageWidth = GroupName + ".MaxImageWidth";

    /// <summary>
    /// Maximum image height
    /// </summary>
    public const string MaxImageHeight = GroupName + ".MaxImageHeight";

    /// <summary>
    /// Auto-delete temporary media after X days
    /// </summary>
    public const string AutoDeleteTempMediaAfterDays = GroupName + ".AutoDeleteTempMediaAfterDays";

    /// <summary>
    /// Enable duplicate detection based on content hash
    /// </summary>
    public const string EnableDuplicateDetection = GroupName + ".EnableDuplicateDetection";
}
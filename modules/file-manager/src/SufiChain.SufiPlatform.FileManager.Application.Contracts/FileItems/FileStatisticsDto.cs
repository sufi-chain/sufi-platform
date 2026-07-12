namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Statistics about file items for the current tenant
/// </summary>
public class FileStatisticsDto
{
    /// <summary>
    /// Total number of files
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Number of image files
    /// </summary>
    public long ImageCount { get; set; }

    /// <summary>
    /// Number of video files
    /// </summary>
    public long VideoCount { get; set; }

    /// <summary>
    /// Number of document files
    /// </summary>
    public long DocumentCount { get; set; }

    /// <summary>
    /// Number of audio files
    /// </summary>
    public long AudioCount { get; set; }

    /// <summary>
    /// Number of other file types
    /// </summary>
    public long OtherCount { get; set; }

    /// <summary>
    /// Total storage used in bytes
    /// </summary>
    public long TotalSize { get; set; }
}

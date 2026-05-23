namespace SufiChain.SufiAbp.FileManager.Settings;

/// <summary>
/// Settings for file archiving
/// </summary>
public static class FileArchivingSettings
{
    private const string Prefix = "FileManager.Archiving";

    /// <summary>
    /// Whether automatic archiving is enabled
    /// Default: true
    /// </summary>
    public const string Enabled = Prefix + ".Enabled";

    /// <summary>
    /// Number of days after which files should be archived
    /// Default: 90
    /// </summary>
    public const string RetentionDays = Prefix + ".RetentionDays";

    /// <summary>
    /// Batch size for archiving operations
    /// Default: 100
    /// </summary>
    public const string BatchSize = Prefix + ".BatchSize";

    /// <summary>
    /// Cron expression for scheduling automatic archiving
    /// Default: "0 2 * * *" (daily at 2 AM)
    /// </summary>
    public const string Schedule = Prefix + ".Schedule";

    /// <summary>
    /// Whether to archive files from AIManagement module
    /// Default: true
    /// </summary>
    public const string ArchiveAIFiles = Prefix + ".ArchiveAIFiles";

    /// <summary>
    /// Retention days specifically for AI files (overrides RetentionDays if set)
    /// Default: null (uses RetentionDays)
    /// </summary>
    public const string AIFilesRetentionDays = Prefix + ".AIFilesRetentionDays";
}

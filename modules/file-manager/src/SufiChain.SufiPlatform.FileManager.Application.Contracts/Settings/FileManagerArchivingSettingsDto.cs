namespace SufiChain.SufiPlatform.FileManager.Settings;

/// <summary>
/// Tenant-level file archiving settings.
/// </summary>
public class FileManagerArchivingSettingsDto
{
    public bool Enabled { get; set; }

    public int RetentionDays { get; set; }

    public int BatchSize { get; set; }

    /// <summary>
    /// Cron expression for scheduling automatic archiving (for example "0 2 * * *").
    /// </summary>
    public string Schedule { get; set; } = "0 2 * * *";

    public bool ArchiveAIFiles { get; set; }

    /// <summary>
    /// Optional override for AI file retention. When null, uses <see cref="RetentionDays"/>.
    /// </summary>
    public int? AIFilesRetentionDays { get; set; }
}

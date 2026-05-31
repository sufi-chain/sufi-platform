namespace SufiChain.SufiAbp.FileManager.Features;

/// <summary>
/// Shared feature names for SufiAbp File Manager capabilities.
/// </summary>
public static class SufiAbpFileManagerFeatures
{
    public const string GroupName = "FileManager";

    /// <summary>
    /// Master switch for the File Manager module.
    /// </summary>
    public const string Enable = GroupName + ".Enable";

    /// <summary>
    /// File and folder upload, download, and asset management.
    /// </summary>
    public const string FileItems = GroupName + ".FileItems";

    /// <summary>
    /// File structure definitions and storage routing.
    /// </summary>
    public const string FileStructures = GroupName + ".FileStructures";

    /// <summary>
    /// Per-structure storage provider configuration.
    /// </summary>
    public const string StorageSettings = GroupName + ".StorageSettings";

    /// <summary>
    /// Automatic file archiving background jobs.
    /// </summary>
    public const string Archiving = GroupName + ".Archiving";
}

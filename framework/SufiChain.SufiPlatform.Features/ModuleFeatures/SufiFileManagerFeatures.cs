namespace SufiChain.SufiPlatform.FileManager.Features;

/// <summary>
/// Shared feature names for Sufi File Manager capabilities.
/// </summary>
public static class SufiFileManagerFeatures
{
    public const string GroupName = "SufiFileManager";

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

    /// <summary>
    /// Storage entitlement features shared by editions and tenants.
    /// </summary>
    public static class Storage
    {
        public const string Provider = GroupName + ".Storage.Provider";
        public const string MaxBytes = GroupName + ".Storage.MaxBytes";

        public const string DefaultProvider = Providers.Database;
        public const string DefaultMaxBytes = "100000000";

        public static class Providers
        {
            public const string Database = "Database";
            public const string FileSystem = "FileSystem";
            public const string MinIO = "MinIO";
            public const string S3Provider = "S3Provider";
        }
    }
}

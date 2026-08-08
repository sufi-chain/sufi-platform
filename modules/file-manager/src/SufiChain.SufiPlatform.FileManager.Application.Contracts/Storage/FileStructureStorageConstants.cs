namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// ExtraProperties keys for file structure storage configuration
/// </summary>
public static class FileStructureStorageConstants
{
    public const string Provider = "Storage.Provider";
    public const string DatabaseConnectionString = "Storage.Database.ConnectionString";
    public const string FileSystemBasePath = "Storage.FileSystem.BasePath";
    public const string MinioEndPoint = "Storage.MinIO.EndPoint";
    public const string MinioAccessKey = "Storage.MinIO.AccessKey";
    public const string MinioSecretKey = "Storage.MinIO.SecretKey";
    public const string MinioBucketName = "Storage.MinIO.BucketName";

    public const string S3Endpoint = "Storage.S3.Endpoint";
    public const string S3Region = "Storage.S3.Region";
    public const string S3AccessKeyId = "Storage.S3.AccessKeyId";
    public const string S3SecretAccessKey = "Storage.S3.SecretAccessKey";
    public const string S3ContainerName = "Storage.S3.ContainerName";

    /// <summary>
    /// Default blob container name when no structure-specific config exists
    /// </summary>
    public const string DefaultContainerName = "sufi-file-manager";

    /// <summary>
    /// Container name prefix for structure-specific storage
    /// </summary>
    public const string ContainerNamePrefix = "sufi-file-manager-";

    /// <summary>
    /// Standardized root path for file-manager FileSystem storage.
    /// Paths are: assets/{structure-name}/{host|tenant}/{year}/{month} or assets/{custom-path}.
    /// </summary>
    public const string AssetsPathPrefix = "assets";
}

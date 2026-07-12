namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// DTO for creating/updating file structure storage configuration.
/// Sensitive values (ConnectionString, AccessKey, SecretKey) should be encrypted before storing.
/// </summary>
public class FileStructureStorageConfigDto
{
    /// <summary>
    /// Storage provider type
    /// </summary>
    public FileStructureStorageProvider StorageProvider { get; set; } = FileStructureStorageProvider.Database;

    /// <summary>
    /// Database connection string. Null = use default connection.
    /// Encrypted when stored.
    /// </summary>
    public string? DatabaseConnectionString { get; set; }

    /// <summary>
    /// File system base path (required when provider is FileSystem)
    /// </summary>
    public string? FileSystemBasePath { get; set; }

    /// <summary>
    /// MinIO endpoint URL
    /// </summary>
    public string? MinioEndPoint { get; set; }

    /// <summary>
    /// MinIO access key. Encrypted when stored.
    /// </summary>
    public string? MinioAccessKey { get; set; }

    /// <summary>
    /// MinIO secret key. Encrypted when stored.
    /// </summary>
    public string? MinioSecretKey { get; set; }

    /// <summary>
    /// MinIO bucket name
    /// </summary>
    public string? MinioBucketName { get; set; }

    /// <summary>
    /// Indicates that a sensitive value (ConnectionString, AccessKey, or SecretKey) is configured.
    /// Used for UI to show placeholder instead of decrypted value.
    /// </summary>
    public bool HasDatabaseConnectionString { get; set; }

    /// <summary>
    /// Indicates MinIO AccessKey is configured
    /// </summary>
    public bool HasMinioAccessKey { get; set; }

    /// <summary>
    /// Indicates MinIO SecretKey is configured
    /// </summary>
    public bool HasMinioSecretKey { get; set; }

    /// <summary>
    /// S3-compatible endpoint URL (optional for AWS; required for MinIO, DigitalOcean Spaces, etc.)
    /// </summary>
    public string? S3EndPoint { get; set; }

    /// <summary>
    /// S3 region (e.g. us-east-1)
    /// </summary>
    public string? S3Region { get; set; }

    /// <summary>
    /// S3 access key. Encrypted when stored.
    /// </summary>
    public string? S3AccessKeyId { get; set; }

    /// <summary>
    /// S3 secret access key. Encrypted when stored.
    /// </summary>
    public string? S3SecretAccessKey { get; set; }

    /// <summary>
    /// S3 bucket/container name
    /// </summary>
    public string? S3ContainerName { get; set; }

    /// <summary>
    /// Indicates S3 AccessKeyId is configured
    /// </summary>
    public bool HasS3AccessKey { get; set; }

    /// <summary>
    /// Indicates S3 SecretAccessKey is configured
    /// </summary>
    public bool HasS3SecretKey { get; set; }
}

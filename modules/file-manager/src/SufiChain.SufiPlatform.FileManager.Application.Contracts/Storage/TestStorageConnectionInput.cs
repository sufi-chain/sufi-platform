namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Input for testing storage connection. Contains provider type and provider-specific credentials/config.
/// </summary>
public class TestStorageConnectionInput
{
    public FileStructureStorageProvider StorageProvider { get; set; }

    public string? DatabaseConnectionString { get; set; }
    public string? FileSystemBasePath { get; set; }
    public string? MinioEndPoint { get; set; }
    public string? MinioAccessKey { get; set; }
    public string? MinioSecretKey { get; set; }
    public string? MinioBucketName { get; set; }
    public string? S3EndPoint { get; set; }
    public string? S3Region { get; set; }
    public string? S3AccessKeyId { get; set; }
    public string? S3SecretAccessKey { get; set; }
    public string? S3ContainerName { get; set; }
}

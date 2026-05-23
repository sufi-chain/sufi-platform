namespace SufiChain.SufiAbp.FileManager.Storage;

/// <summary>
/// Built-in storage providers for file structure blob storage
/// </summary>
public enum FileStructureStorageProvider
{
    Database,
    FileSystem,
    MinIO,
    S3Provider
}

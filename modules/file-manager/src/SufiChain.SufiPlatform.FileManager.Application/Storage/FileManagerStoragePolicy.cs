namespace SufiChain.SufiPlatform.FileManager.Storage;

public class FileManagerStoragePolicy
{
    public FileStructureStorageProvider Provider { get; init; }

    public long MaxStorageBytes { get; init; }
}

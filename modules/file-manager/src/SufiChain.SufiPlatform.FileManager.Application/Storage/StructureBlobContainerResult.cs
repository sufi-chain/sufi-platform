using Volo.Abp.BlobStoring;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public sealed class StructureBlobContainerResult
{
    public IBlobContainer Container { get; }

    public FileStructureStorageProvider StorageProvider { get; }

    public StructureBlobContainerResult(
        IBlobContainer container,
        FileStructureStorageProvider storageProvider)
    {
        Container = container;
        StorageProvider = storageProvider;
    }
}

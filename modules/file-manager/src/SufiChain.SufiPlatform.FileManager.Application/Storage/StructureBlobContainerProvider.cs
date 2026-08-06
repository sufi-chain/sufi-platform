using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class StructureBlobContainerProvider : IStructureBlobContainerProvider, ITransientDependency
{
    private readonly IBlobContainerFactory _blobContainerFactory;

    public StructureBlobContainerProvider(IBlobContainerFactory blobContainerFactory)
    {
        _blobContainerFactory = blobContainerFactory;
    }

    public Task<IBlobContainer> GetContainerAsync(string? structureKey, CancellationToken cancellationToken = default)
    {
        var containerName = string.IsNullOrEmpty(structureKey)
            ? FileStructureStorageConstants.DefaultContainerName
            : FileStructureStorageConstants.ContainerNamePrefix + structureKey;

        var container = _blobContainerFactory.Create(containerName);
        return Task.FromResult(container);
    }
}

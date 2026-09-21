using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.Storage;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.FileManager.FileMigration;

public class FileBlobMigrationService : ITransientDependency
{
    private readonly IStructureBlobContainerProvider _containerProvider;
    private readonly IFileItemRepository _fileItemRepository;

    public FileBlobMigrationService(
        IStructureBlobContainerProvider containerProvider,
        IFileItemRepository fileItemRepository)
    {
        _containerProvider = containerProvider;
        _fileItemRepository = fileItemRepository;
    }

    public virtual async Task<bool> MigrateFileAsync(
        FileItem item,
        string? targetStructureKey = null,
        Guid? targetFolderId = null,
        bool deleteSourceAfterVerify = true,
        CancellationToken cancellationToken = default)
    {
        var destinationKey = string.IsNullOrWhiteSpace(targetStructureKey)
            ? item.StructureKey
            : targetStructureKey;
        var write = await _containerProvider.GetWriteContainerAsync(destinationKey, cancellationToken);

        var sameStructure = string.Equals(item.StructureKey, destinationKey, StringComparison.OrdinalIgnoreCase);
        if (sameStructure && item.StorageProvider == write.StorageProvider)
        {
            return false;
        }

        var source = await _containerProvider.GetContainerAsync(
            item.StructureKey,
            item.StorageProvider,
            cancellationToken);

        await CopyBlobIfMissingAsync(source, write.Container, item.BlobName, cancellationToken);
        if (!string.IsNullOrWhiteSpace(item.ThumbnailBlobName))
        {
            await CopyBlobIfMissingAsync(source, write.Container, item.ThumbnailBlobName, cancellationToken);
        }

        if (!await write.Container.ExistsAsync(item.BlobName, cancellationToken))
        {
            throw new InvalidOperationException($"Destination blob was not verified: {item.BlobName}");
        }

        item.StructureKey = destinationKey;
        if (targetFolderId.HasValue || !sameStructure)
        {
            item.FolderId = targetFolderId;
        }

        item.SetStorageProvider(write.StorageProvider);
        await _fileItemRepository.UpdateAsync(item, autoSave: true, cancellationToken);

        if (deleteSourceAfterVerify && !ReferenceEquals(source, write.Container))
        {
            await TryDeleteAsync(source, item.BlobName, cancellationToken);
            if (!string.IsNullOrWhiteSpace(item.ThumbnailBlobName))
            {
                await TryDeleteAsync(source, item.ThumbnailBlobName, cancellationToken);
            }
        }

        return true;
    }

    private static async Task CopyBlobIfMissingAsync(
        IBlobContainer source,
        IBlobContainer destination,
        string blobName,
        CancellationToken cancellationToken)
    {
        if (await destination.ExistsAsync(blobName, cancellationToken))
        {
            return;
        }

        await using var stream = await source.GetOrNullAsync(blobName, cancellationToken);
        if (stream == null)
        {
            throw new InvalidOperationException($"Source blob was not found: {blobName}");
        }

        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        buffer.Position = 0;
        await destination.SaveAsync(blobName, buffer, overrideExisting: true, cancellationToken);
    }

    private static async Task TryDeleteAsync(
        IBlobContainer container,
        string blobName,
        CancellationToken cancellationToken)
    {
        try
        {
            await container.DeleteAsync(blobName, cancellationToken);
        }
        catch (Exception)
        {
            // Source cleanup is best-effort after the destination and database are updated.
        }
    }
}

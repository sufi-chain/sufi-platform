using Volo.Abp.BlobStoring;

namespace SufiChain.SufiPlatform.FileManager.Storage;

/// <summary>
/// Provides blob containers for file structures. Each structure can have its own storage provider configuration.
/// Internal infrastructure abstraction - kept in Application layer, not exposed in Contracts.
/// </summary>
public interface IStructureBlobContainerProvider
{
    /// <summary>
    /// Gets the blob container for the given structure.
    /// When structureKey is null or empty, returns the default container.
    /// </summary>
    Task<IBlobContainer> GetContainerAsync(string? structureKey, CancellationToken cancellationToken = default);
}

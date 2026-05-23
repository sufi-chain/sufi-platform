namespace SufiChain.SufiAbp.FileManager.Caching;

/// <summary>
/// Provides cached access to file structure data, avoiding database queries.
/// </summary>
public interface IStructureCache
{
    /// <summary>
    /// Gets the cached structure by key, or null if not found.
    /// </summary>
    Task<StructureCacheEntry?> GetAsync(string? structureKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns keys of structures where IsPublicAccess is true.
    /// </summary>
    Task<HashSet<string>> GetPublicStructureKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns whether the structure allows public access.
    /// </summary>
    Task<bool> IsPublicAccessAsync(string? structureKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all cached structures by key.
    /// </summary>
    Task<IReadOnlyDictionary<string, StructureCacheEntry>> GetAllAsync(CancellationToken cancellationToken = default);
}

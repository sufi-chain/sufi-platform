namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// In-process file storage operations for trusted module code (background jobs, domain services).
/// Never exposed over HTTP. Callers must authorize the owning entity in their own domain before calling.
/// Structure validation (types, size, folder ownership) still applies.
/// </summary>
public interface IFileStorageTrustedService
{
    /// <summary>
    /// Ensures a folder exists under a structure. Missing ancestors are created with their path segment as name.
    /// The leaf folder name and localization resource are created or updated to match the request.
    /// </summary>
    Task<FileFolderReferenceDto> EnsureFolderAsync(FileFolderEnsureRequest input);

    /// <summary>Uploads a file without requiring an authenticated user.</summary>
    Task<FileReferenceDto> UploadAsync(FileUploadRequest input);

    /// <summary>Lists files owned by an entity, optionally limited to one structure.</summary>
    Task<List<FileReferenceDto>> GetListByEntityAsync(string entityType, Guid entityId, string? structureKey = null);

    /// <summary>Gets a file reference, or <c>null</c> when the file does not exist.</summary>
    Task<FileReferenceDto?> FindAsync(Guid id);

    /// <summary>Reads stored file bytes.</summary>
    Task<FileContentBytesDto> GetContentAsync(Guid id);

    /// <summary>
    /// Merges string properties into the file's extra properties. A <c>null</c> value removes the property.
    /// </summary>
    Task SetPropertiesAsync(Guid id, IReadOnlyDictionary<string, string?> properties);

    /// <summary>Deletes a stored file (blob, thumbnail, and record).</summary>
    Task DeleteAsync(Guid id);
}

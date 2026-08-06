namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Services;

/// <summary>
/// Provides URLs for file manager API endpoints (thumbnail, download, stream).
/// When structure uses S3 with IsPublicAccess and BaseUrl, returns direct S3 public URLs.
/// Uses RemoteServices:FileManager:BaseUrl or RemoteServices:Default:BaseUrl for API URLs.
/// </summary>
public interface IFileItemUrlProvider
{
    /// <summary>
    /// Base URL of the file manager API (e.g. https://localhost:44305/). Never null; may be relative "/".
    /// </summary>
    string ApiBaseUrl { get; }

    /// <summary>
    /// Gets the thumbnail URL for a file item.
    /// When structureStorageProvider is S3Provider, structureIsPublicAccess is true, and structureBaseUrl is set,
    /// returns direct S3 public URL. Otherwise returns API URL (with token when not public).
    /// </summary>
    /// <param name="fileItemId">File item ID.</param>
    /// <param name="cacheBust">Optional value to append so the browser bypasses cache (e.g. after save). Use file.LastModificationTime?.Ticks.</param>
    /// <param name="structureBaseUrl">Optional structure-specific base URL. When null, uses config default.</param>
    /// <param name="structureIsPublicAccess">When true, URL has no token (plain URL for anonymous access).</param>
    /// <param name="thumbnailBlobName">Blob name for thumbnail. When provided with S3Provider+public+baseUrl, builds direct S3 URL.</param>
    /// <param name="tenantId">Tenant ID of the file. Used for S3 path: tenants/{id}/ when set.</param>
    /// <param name="structureStorageProvider">Storage provider (e.g. "S3Provider"). When S3Provider with public+baseUrl, uses direct URL.</param>
    string GetThumbnailUrl(Guid fileItemId, long? cacheBust = null, string? structureBaseUrl = null, bool structureIsPublicAccess = false, string? thumbnailBlobName = null, Guid? tenantId = null, string? structureStorageProvider = null);

    /// <summary>
    /// Gets the download URL for a file item.
    /// </summary>
    /// <param name="structureBaseUrl">Optional structure-specific base URL. When null, uses config default.</param>
    /// <param name="structureIsPublicAccess">When true, URL has no token (plain URL for anonymous access).</param>
    /// <param name="blobName">Blob name. When provided with S3Provider+public+baseUrl, builds direct S3 URL.</param>
    /// <param name="tenantId">Tenant ID of the file.</param>
    /// <param name="structureStorageProvider">Storage provider (e.g. "S3Provider").</param>
    string GetDownloadUrl(Guid fileItemId, string? structureBaseUrl = null, bool structureIsPublicAccess = false, string? blobName = null, Guid? tenantId = null, string? structureStorageProvider = null);

    /// <summary>
    /// Gets the stream URL for a file item (e.g. video/audio).
    /// </summary>
    string GetStreamUrl(Guid fileItemId, long? cacheBust = null, string? structureBaseUrl = null, bool structureIsPublicAccess = false, string? blobName = null, Guid? tenantId = null, string? structureStorageProvider = null);
}

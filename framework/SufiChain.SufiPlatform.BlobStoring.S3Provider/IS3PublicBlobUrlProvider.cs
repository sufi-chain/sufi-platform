namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

/// <summary>
/// Provides direct public URLs for S3 blobs when IsPublicAccess is configured.
/// Use when FileStructure.IsPublicAccess is true so browsers hit object storage (or CDN) directly — no API proxy traffic.
/// </summary>
public interface IS3PublicBlobUrlProvider
{
    /// <summary>
    /// Tries to get the public base URL for the container (configured BaseUrl or derived from S3 endpoint/bucket/region).
    /// Returns true only when the container uses S3 and IsPublicAccess is true.
    /// </summary>
    bool TryGetPublicBaseUrl(string containerName, out string? baseUrl);

    /// <summary>
    /// Tries to get a direct public URL for the blob.
    /// Returns true only when the container uses S3, IsPublicAccess is true, and a public base URL is available.
    /// </summary>
    bool TryGetPublicUrl(string containerName, string blobName, Guid? tenantId, out string? url);
}

namespace SufiChain.SufiAbp.BlobStoring.S3Provider;

/// <summary>
/// Provides direct public URLs for S3 blobs when IsPublicAccess and PublicBaseUrl are configured.
/// Use when FileStructure.IsPublicAccess is true and BaseUrl points to the S3 bucket or CDN.
/// </summary>
public interface IS3PublicBlobUrlProvider
{
    /// <summary>
    /// Tries to get a direct public URL for the blob.
    /// Returns true only when the container uses S3, IsPublicAccess is true, and PublicBaseUrl is set.
    /// </summary>
    bool TryGetPublicUrl(string containerName, string blobName, Guid? tenantId, out string? url);
}

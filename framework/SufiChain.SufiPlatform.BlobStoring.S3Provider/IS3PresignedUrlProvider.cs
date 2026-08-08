using System;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.BlobStoring.S3Provider;

/// <summary>
/// Generates presigned download URLs for S3 blobs when IsPublicAccess is false and PublicBaseUrl is configured.
/// Use when the file structure uses S3, BaseUrl is set, and files are private (require temporary access).
/// </summary>
public interface IS3PresignedUrlProvider
{
    /// <summary>
    /// Gets a presigned download URL for the blob with the specified validity period.
    /// Returns null when the container is not S3, IsPublicAccess is true, or config/credentials are invalid.
    /// </summary>
    /// <param name="containerName">Blob container name (e.g. sufi-file-manager-general).</param>
    /// <param name="blobName">Blob name (e.g. 2026/02/file-id.png).</param>
    /// <param name="tenantId">Tenant ID for path; null for host.</param>
    /// <param name="validity">How long the URL is valid. Clamped to 1 minute–7 days.</param>
    Task<string?> GetPresignedDownloadUrlAsync(string containerName, string blobName, Guid? tenantId, TimeSpan validity);
}

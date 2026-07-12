namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Minimal metadata for serving stream/thumbnail/download without full GetAsync.
/// Used when access is validated via token (img/video/links don't send auth headers).
/// </summary>
public class FileStreamMetadataDto
{
    public string BlobName { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string? ThumbnailBlobName { get; set; }
    /// <summary>Original file name for download Content-Disposition.</summary>
    public string OriginalName { get; set; } = string.Empty;
    /// <summary>Structure key for resolving the correct blob container.</summary>
    public string? StructureKey { get; set; }
    /// <summary>Tenant ID of the file; required for correct blob path resolution (host vs tenants/{id}/).</summary>
    public Guid? TenantId { get; set; }
}

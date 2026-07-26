namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// Cross-module file reference (id + display metadata + optional access token).
/// Prefer this over embedding full file-manager DTOs in other modules.
/// </summary>
[Serializable]
public class FileReferenceDto
{
    /// <summary>Stored file id.</summary>
    public Guid Id { get; set; }

    /// <summary>Original file name as uploaded by the user.</summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>MIME type.</summary>
    public string MimeType { get; set; } = string.Empty;

    /// <summary>Size in bytes.</summary>
    public long SizeInBytes { get; set; }

    /// <summary>Optional signed access token for media URLs.</summary>
    public string? AccessToken { get; set; }

    /// <summary>Optional public or relative URL when available.</summary>
    public string? Url { get; set; }

    /// <summary>File structure key (e.g. Chat.Attachments).</summary>
    public string? StructureKey { get; set; }

    /// <summary>Optional owning entity type (e.g. Chat.Session).</summary>
    public string? EntityType { get; set; }

    /// <summary>Optional owning entity id.</summary>
    public Guid? EntityId { get; set; }

    /// <summary>Owning tenant (null for host).</summary>
    public Guid? TenantId { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// Deduplicated upload shape shared by Chat, Knowledge Base, Ticketing, and other consumers.
/// </summary>
[Serializable]
public class FileUploadRequest
{
    [Required]
    public string FileName { get; set; } = string.Empty;

    [Required]
    public byte[] Content { get; set; } = Array.Empty<byte>();

    [Required]
    public string MimeType { get; set; } = string.Empty;

    public string? StructureKey { get; set; }

    public string? EntityType { get; set; }

    public Guid? EntityId { get; set; }

    public Guid? FolderId { get; set; }

    public string? FolderPath { get; set; }

    public bool AutoConfirm { get; set; }

    public string? Alt { get; set; }
}

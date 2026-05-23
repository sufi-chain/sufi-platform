using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.FileManager.FileItems;

/// <summary>
/// Input for Save As - create a new file from edited content (e.g., from image editor).
/// Client sends the final edited bytes; backend creates a new file.
/// </summary>
public class SaveAsFileInput
{
    [Required]
    public Guid SourceId { get; set; }

    [Required]
    public string FileName { get; set; } = default!;

    [Required]
    public byte[] Content { get; set; } = default!;

    [Required]
    public string MimeType { get; set; } = default!;

    /// <summary>
    /// Target folder ID. If null, uses the source file's folder or root.
    /// </summary>
    public Guid? FolderId { get; set; }
}

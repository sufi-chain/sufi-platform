using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.FileManager.FileItems;

/// <summary>
/// Input for replacing file content (e.g., after frontend image editing).
/// Backend stores bytes as-is; no image transformation.
/// </summary>
public class ReplaceFileContentInput
{
    [Required]
    public byte[] Content { get; set; } = default!;

    /// <summary>
    /// Optional. If provided, updates the file's display name. If null, keeps original.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Optional. If provided, updates MIME type. If null, inferred from content or current file.
    /// </summary>
    public string? MimeType { get; set; }
}

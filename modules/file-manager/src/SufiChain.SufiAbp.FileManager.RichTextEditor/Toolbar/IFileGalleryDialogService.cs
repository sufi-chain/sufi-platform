using System.Threading.Tasks;

namespace SufiChain.SufiAbp.FileManager.RichTextEditor.Toolbar;

/// <summary>
/// Service for showing file gallery dialogs in the rich text editor.
/// </summary>
public interface IFileGalleryDialogService
{
    /// <summary>
    /// Whether a FileGalleryHost component is currently rendered and registered.
    /// Toolbar contributors use this to hide file manager buttons when the host is absent.
    /// </summary>
    bool IsHostRegistered { get; }

    /// <summary>
    /// Shows a dialog for selecting an image from the file gallery.
    /// </summary>
    /// <returns>The selected file result, or null if cancelled.</returns>
    Task<FileGalleryResult?> ShowImageGalleryAsync();

    /// <summary>
    /// Shows a dialog for selecting any file from the file gallery.
    /// </summary>
    /// <returns>The selected file result, or null if cancelled.</returns>
    Task<FileGalleryResult?> ShowFileGalleryAsync();
}

/// <summary>
/// Result from selecting a file in the gallery dialog.
/// </summary>
public class FileGalleryResult
{
    /// <summary>
    /// The file ID.
    /// </summary>
    public Guid FileId { get; set; }

    /// <summary>
    /// The URL to access the file.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// The file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// The alt text for images.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// The MIME type.
    /// </summary>
    public string? MimeType { get; set; }
}

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// File content as bytes for download/thumbnail. Used by app service for in-process transfer to controller.
/// </summary>
public class FileContentDto
{
    public byte[] Content { get; set; } = [];
    public string MimeType { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
}

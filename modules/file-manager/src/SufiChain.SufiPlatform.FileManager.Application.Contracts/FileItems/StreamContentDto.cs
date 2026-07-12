using System.IO;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// File content as stream for streaming (e.g. video). Caller must ensure stream is disposed after use.
/// Used by app service for in-process transfer to controller; not serializable.
/// </summary>
public class StreamContentDto
{
    public Stream Stream { get; set; } = null!;
    public string MimeType { get; set; } = string.Empty;
}

namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// Portable file bytes for cross-module consumers (import, conversion, indexing).
/// </summary>
[Serializable]
public class FileContentBytesDto
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string MimeType { get; set; } = string.Empty;

    public byte[] Content { get; set; } = Array.Empty<byte>();
}

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Result of a file content request. Use IsForbidden for 403, null Content for 404.
/// </summary>
public class FileContentResultDto
{
    public FileContentDto? Content { get; set; }
    public bool IsForbidden { get; set; }
}

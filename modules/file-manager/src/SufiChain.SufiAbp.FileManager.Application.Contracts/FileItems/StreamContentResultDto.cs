namespace SufiChain.SufiAbp.FileManager.FileItems;

/// <summary>
/// Result of a stream content request. Use IsForbidden for 403, null Content for 404.
/// </summary>
public class StreamContentResultDto
{
    public StreamContentDto? Content { get; set; }
    public bool IsForbidden { get; set; }
}

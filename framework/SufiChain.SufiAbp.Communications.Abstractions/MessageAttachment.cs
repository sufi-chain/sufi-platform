namespace SufiChain.SufiAbp.Communications;

/// <summary>
/// Represents an attachment for messages (primarily for emails)
/// </summary>
public class MessageAttachment
{
    public byte[] File { get; }
    
    public string FileName { get; }
    
    public string? ContentType { get; set; }
    
    public string? ContentId { get; set; }

    public MessageAttachment(byte[] file, string fileName, string? contentType = null, string? contentId = null)
    {
        File = file;
        FileName = fileName;
        ContentType = contentType;
        ContentId = contentId;
    }
}

namespace SufiChain.Chat.Composer;

public class ChatComposerSendRequest
{
    public string Body { get; set; } = string.Empty;

    public List<Guid> AttachmentFileIds { get; set; } = new();

    public string? MetadataJson { get; set; }
}

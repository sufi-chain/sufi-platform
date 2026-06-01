namespace SufiChain.Chat.Connectors.Inbound;

/// <summary>
/// Result of inbound connector message ingest.
/// </summary>
public class IngestInboundChatMessageResult
{
    public Guid SessionId { get; set; }

    public Guid MessageId { get; set; }

    public bool CreatedNewSession { get; set; }
}

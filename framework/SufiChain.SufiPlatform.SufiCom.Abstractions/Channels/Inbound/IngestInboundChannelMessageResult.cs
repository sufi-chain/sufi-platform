namespace SufiChain.SufiPlatform.SufiCom.Channels.Inbound;

/// <summary>
/// Result of inbound connector message ingest.
/// </summary>
public class IngestInboundChannelMessageResult
{
    public Guid SessionId { get; set; }
    public Guid MessageId { get; set; }
    public bool CreatedNewSession { get; set; }
}
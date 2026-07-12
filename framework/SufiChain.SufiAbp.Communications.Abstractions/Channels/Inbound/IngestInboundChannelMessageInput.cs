using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Communications.Channels.Inbound;

/// <summary>
/// Generic inbound message ingest request from a channel connector.
/// </summary>
public class IngestInboundChannelMessageInput
{
    [Required]
    [StringLength(ChannelConsts.MaxConnectorNameLength)]
    public string ConnectorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string ExternalThreadId { get; set; } = string.Empty;

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? ExternalMessageId { get; set; }

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? InReplyToExternalMessageId { get; set; }

    [StringLength(ChannelConsts.MaxTitleLength)]
    public string? Title { get; set; }

    public AccessMode AccessMode { get; set; } = AccessMode.PublicAnonymous;

    public ConversationKind? ConversationKind { get; set; }

    [Required]
    [StringLength(ChannelConsts.MaxMessageBodyLength)]
    public string Body { get; set; } = string.Empty;

    public ChannelInboundSenderInput Sender { get; set; } = new();

    [StringLength(ChannelConsts.MaxExternalIdLength)]
    public string? ExternalParticipantAddress { get; set; }

    [StringLength(ChannelConsts.MaxDisplayNameLength)]
    public string? ExternalParticipantName { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public List<Guid> AttachmentFileIds { get; set; } = new();
}

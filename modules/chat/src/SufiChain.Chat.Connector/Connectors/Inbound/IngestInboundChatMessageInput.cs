using System.ComponentModel.DataAnnotations;

namespace SufiChain.Chat.Connectors.Inbound;

/// <summary>
/// Generic inbound message ingest request from a channel connector.
/// </summary>
public class IngestInboundChatMessageInput
{
    [Required]
    [StringLength(ChatConsts.MaxConnectorNameLength)]
    public string ConnectorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string ExternalThreadId { get; set; } = string.Empty;

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? ExternalMessageId { get; set; }

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? InReplyToExternalMessageId { get; set; }

    [StringLength(ChatConsts.MaxTitleLength)]
    public string? Title { get; set; }

    public AccessMode AccessMode { get; set; } = AccessMode.PublicAnonymous;

    public ConversationKind? ConversationKind { get; set; }

    [Required]
    [StringLength(ChatConsts.MaxMessageBodyLength)]
    public string Body { get; set; } = string.Empty;

    public ChatInboundSenderInput Sender { get; set; } = new();

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? AdditionalMetadataJson { get; set; }

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? ExternalParticipantAddress { get; set; }

    [StringLength(ChatConsts.MaxDisplayNameLength)]
    public string? ExternalParticipantName { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public List<Guid> AttachmentFileIds { get; set; } = new();
}

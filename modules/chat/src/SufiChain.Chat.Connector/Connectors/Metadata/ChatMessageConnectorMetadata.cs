using System.ComponentModel.DataAnnotations;

namespace SufiChain.Chat.Connectors.Metadata;

/// <summary>
/// Connector-specific message metadata stored in <c>ChatMessage.MetadataJson</c>.
/// </summary>
public class ChatMessageConnectorMetadata
{
    [Required]
    [StringLength(ChatConsts.MaxConnectorNameLength)]
    public string ConnectorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string ExternalMessageId { get; set; } = string.Empty;

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? InReplyToExternalMessageId { get; set; }
}

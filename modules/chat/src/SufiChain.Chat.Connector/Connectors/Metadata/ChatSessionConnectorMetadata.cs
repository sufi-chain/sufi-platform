using System.ComponentModel.DataAnnotations;

namespace SufiChain.Chat.Connectors.Metadata;

/// <summary>
/// Connector-specific session metadata stored in <c>ChatSession.MetadataJson</c>.
/// </summary>
public class ChatSessionConnectorMetadata
{
    [Required]
    [StringLength(ChatConsts.MaxConnectorNameLength)]
    public string ConnectorName { get; set; } = string.Empty;

    [Required]
    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string ExternalThreadId { get; set; } = string.Empty;

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? LastExternalMessageId { get; set; }

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? InReplyToExternalMessageId { get; set; }

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? ExternalParticipantAddress { get; set; }

    [StringLength(ChatConsts.MaxDisplayNameLength)]
    public string? ExternalParticipantName { get; set; }
}

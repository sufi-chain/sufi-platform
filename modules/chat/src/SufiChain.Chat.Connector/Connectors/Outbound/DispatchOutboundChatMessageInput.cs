using System.ComponentModel.DataAnnotations;

namespace SufiChain.Chat.Connectors.Outbound;

/// <summary>
/// Outbound message dispatch request from Chat to a channel connector.
/// </summary>
public class DispatchOutboundChatMessageInput
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    public Guid MessageId { get; set; }

    [Required]
    [StringLength(ChatConsts.MaxMessageBodyLength)]
    public string Body { get; set; } = string.Empty;

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? ExternalThreadId { get; set; }

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? ReplyToExternalMessageId { get; set; }

    public Guid? OperatorUserId { get; set; }

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? MetadataJson { get; set; }

    [StringLength(ChatConsts.MaxMetadataJsonLength)]
    public string? SessionMetadataJson { get; set; }
}

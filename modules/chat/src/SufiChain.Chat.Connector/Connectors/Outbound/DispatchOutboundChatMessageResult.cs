using System.ComponentModel.DataAnnotations;

namespace SufiChain.Chat.Connectors.Outbound;

/// <summary>
/// Result of outbound connector dispatch.
/// </summary>
public class DispatchOutboundChatMessageResult
{
    public bool Succeeded { get; set; }

    [StringLength(ChatConsts.MaxExternalIdLength)]
    public string? ExternalMessageId { get; set; }

    public string? FailureReason { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace SufiChain.Chat.Connectors.Inbound;

/// <summary>
/// Sender identity for an inbound connector message.
/// </summary>
public class ChatInboundSenderInput
{
    public Guid? UserId { get; set; }

    [StringLength(ChatConsts.MaxAnonymousVisitorIdLength)]
    public string? AnonymousVisitorId { get; set; }

    public ChatMessageSenderKind SenderKind { get; set; } = ChatMessageSenderKind.Visitor;

    [StringLength(ChatConsts.MaxDisplayNameLength)]
    public string? DisplayName { get; set; }
}

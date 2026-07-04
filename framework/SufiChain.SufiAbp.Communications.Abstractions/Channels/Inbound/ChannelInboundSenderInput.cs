using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiAbp.Communications.Channels.Inbound;

/// <summary>
/// Sender identity for an inbound connector message.
/// </summary>
public class ChannelInboundSenderInput
{
    public Guid? UserId { get; set; }

    [StringLength(ChannelConsts.MaxAnonymousVisitorIdLength)]
    public string? AnonymousVisitorId { get; set; }

    public ChatMessageSenderKind SenderKind { get; set; } = ChatMessageSenderKind.Visitor;

    [StringLength(ChannelConsts.MaxDisplayNameLength)]
    public string? DisplayName { get; set; }
}
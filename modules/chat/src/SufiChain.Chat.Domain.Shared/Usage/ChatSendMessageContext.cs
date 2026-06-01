using System;

namespace SufiChain.Chat.Usage;

public class ChatSendMessageContext
{
    public Guid? TenantId { get; set; }

    public Guid SessionId { get; set; }

    public Guid? UserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public AccessMode AccessMode { get; set; }

    public ChatMessageSenderKind SenderKind { get; set; }
}

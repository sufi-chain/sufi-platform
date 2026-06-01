using System;

namespace SufiChain.Chat.Events;

public class ChatMessageSentEvent
{
    public Guid MessageId { get; }

    public Guid SessionId { get; }

    public Guid? TenantId { get; }

    public ChatMessageSentEvent(Guid messageId, Guid sessionId, Guid? tenantId)
    {
        MessageId = messageId;
        SessionId = sessionId;
        TenantId = tenantId;
    }
}

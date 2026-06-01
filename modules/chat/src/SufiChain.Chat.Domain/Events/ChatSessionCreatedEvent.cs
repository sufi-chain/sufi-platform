using System;

namespace SufiChain.Chat.Events;

public class ChatSessionCreatedEvent
{
    public Guid SessionId { get; }

    public Guid? TenantId { get; }

    public ChatSessionCreatedEvent(Guid sessionId, Guid? tenantId)
    {
        SessionId = sessionId;
        TenantId = tenantId;
    }
}

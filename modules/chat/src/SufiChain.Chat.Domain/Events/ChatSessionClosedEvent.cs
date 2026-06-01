using System;

namespace SufiChain.Chat.Events;

public class ChatSessionClosedEvent
{
    public Guid SessionId { get; }

    public Guid? TenantId { get; }

    public Guid? ClosedByUserId { get; }

    public ChatSessionClosedEvent(Guid sessionId, Guid? tenantId, Guid? closedByUserId)
    {
        SessionId = sessionId;
        TenantId = tenantId;
        ClosedByUserId = closedByUserId;
    }
}

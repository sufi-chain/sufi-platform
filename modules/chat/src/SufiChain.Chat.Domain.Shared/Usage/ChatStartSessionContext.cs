using System;

namespace SufiChain.Chat.Usage;

public class ChatStartSessionContext
{
    public Guid? TenantId { get; set; }

    public Guid? UserId { get; set; }

    public string? AnonymousVisitorId { get; set; }

    public string? AnonymousClientIpHash { get; set; }

    public AccessMode AccessMode { get; set; }

    public ConversationKind ConversationKind { get; set; }

    public ChannelOrigin ChannelOrigin { get; set; }

    public string? SourceEntityType { get; set; }

    public string? SourceEntityId { get; set; }
}

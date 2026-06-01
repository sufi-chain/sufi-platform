using System;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.EventBus;

namespace SufiChain.Chat.ETOs;

[Serializable]
[EventName("SufiChain.Chat.SessionCreated")]
public class ChatSessionCreatedEto : IMultiTenant
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string? Title { get; set; }
    public AccessMode AccessMode { get; set; }
    public ConversationKind ConversationKind { get; set; }
    public ChannelOrigin ChannelOrigin { get; set; }
    public DateTime CreatedAt { get; set; }
}

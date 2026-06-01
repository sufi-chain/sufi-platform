using System;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.EventBus;

namespace SufiChain.Chat.ETOs;

[Serializable]
[EventName("SufiChain.Chat.MessageSent")]
public class ChatMessageSentEto : IMultiTenant
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid SessionId { get; set; }
    public ChatMessageSenderKind SenderKind { get; set; }
    public Guid? SenderUserId { get; set; }
    public string? AnonymousVisitorId { get; set; }
    public bool IsInternal { get; set; }
    public DateTime SentAt { get; set; }
}

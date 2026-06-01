using System;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.EventBus;

namespace SufiChain.Chat.ETOs;

[Serializable]
[EventName("SufiChain.Chat.SessionClosed")]
public class ChatSessionClosedEto : IMultiTenant
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public DateTime ClosedAt { get; set; }
}

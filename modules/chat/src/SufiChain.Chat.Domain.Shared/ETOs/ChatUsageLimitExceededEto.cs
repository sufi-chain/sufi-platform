using System;
using SufiChain.SufiAbp.MultiTenancy;
using Volo.Abp.EventBus;

namespace SufiChain.Chat.ETOs;

[Serializable]
[EventName("SufiChain.Chat.UsageLimitExceeded")]
public class ChatUsageLimitExceededEto : IMultiTenant
{
    public Guid? TenantId { get; set; }
    public Guid? SessionId { get; set; }
    public Guid? UserId { get; set; }
    public ChatAiOperationKind? AiOperationKind { get; set; }
    public string ReasonCode { get; set; } = default!;
    public string? LocalizationKey { get; set; }
    public LimitExceededAction Action { get; set; }
    public DateTime OccurredAt { get; set; }
}

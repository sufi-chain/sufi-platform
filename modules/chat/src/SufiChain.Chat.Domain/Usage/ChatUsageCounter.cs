using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace SufiChain.Chat.Usage;

public class ChatUsageCounter : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual string CounterKey { get; protected set; } = string.Empty;

    public virtual ChatUsageCounterPeriod Period { get; protected set; }

    public virtual DateTime PeriodStart { get; protected set; }

    public virtual DateTime PeriodEnd { get; protected set; }

    public virtual long Count { get; protected set; }

    public virtual long TokenCount { get; protected set; }

    protected ChatUsageCounter()
    {
    }

    public ChatUsageCounter(
        Guid id,
        Guid? tenantId,
        string counterKey,
        ChatUsageCounterPeriod period,
        DateTime periodStart,
        DateTime periodEnd)
        : base(id)
    {
        TenantId = tenantId;
        CounterKey = Check.NotNullOrWhiteSpace(counterKey, nameof(counterKey), ChatConsts.MaxUsageCounterKeyLength);
        Period = period;
        PeriodStart = periodStart;
        PeriodEnd = periodEnd;
    }

    public virtual void Increment(long count = 1, long tokenCount = 0)
    {
        Count += count;
        TokenCount += tokenCount;
    }
}

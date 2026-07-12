using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.Calendar.Events;

public class RecurrenceRule : Entity<Guid>
{
    public virtual Guid EventId { get; private set; }

    public virtual string Rule { get; private set; } = default!;

    public virtual string Frequency { get; private set; } = default!;

    public virtual int Interval { get; private set; }

    public virtual int? Count { get; private set; }

    public virtual DateTime? UntilUtc { get; private set; }

    protected RecurrenceRule()
    {
    }

    public RecurrenceRule(Guid id, Guid eventId, string rule)
        : base(id)
    {
        EventId = eventId;
        SetRule(rule);
    }

    public virtual void SetRule(string rule)
    {
        Rule = Check.NotNullOrWhiteSpace(rule, nameof(rule), EventConsts.MaxRecurrenceRuleLength);

        var parts = RecurrenceRuleParser.Parse(rule);
        Frequency = parts.GetRequired("FREQ", EventConsts.MaxRecurrenceFrequencyLength).ToUpperInvariant();
        Interval = parts.GetOptionalInt("INTERVAL") ?? 1;
        Count = parts.GetOptionalInt("COUNT");
        UntilUtc = parts.GetOptionalUtc("UNTIL");

        if (Interval <= 0 || Count is <= 0 || !RecurrenceRuleParser.IsSupportedFrequency(Frequency))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidRecurrenceRule);
        }
    }
}

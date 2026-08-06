using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class EventOccurrenceException : Entity<Guid>
{
    public virtual Guid EventId { get; private set; }

    public virtual DateTime OriginalStartUtc { get; private set; }

    public virtual bool IsCancelled { get; private set; }

    public virtual DateTime? OverrideStartUtc { get; private set; }

    public virtual DateTime? OverrideEndUtc { get; private set; }

    public virtual bool ThisAndFollowing { get; private set; }

    protected EventOccurrenceException()
    {
    }

    private EventOccurrenceException(Guid id, Guid eventId, DateTime originalStartUtc, bool isCancelled, DateTime? overrideStartUtc, DateTime? overrideEndUtc, bool thisAndFollowing)
        : base(id)
    {
        EventId = eventId;
        OriginalStartUtc = DateTime.SpecifyKind(originalStartUtc, DateTimeKind.Utc);
        IsCancelled = isCancelled;
        OverrideStartUtc = overrideStartUtc.HasValue ? DateTime.SpecifyKind(overrideStartUtc.Value, DateTimeKind.Utc) : null;
        OverrideEndUtc = overrideEndUtc.HasValue ? DateTime.SpecifyKind(overrideEndUtc.Value, DateTimeKind.Utc) : null;
        ThisAndFollowing = thisAndFollowing;

        Validate();
    }

    public static EventOccurrenceException Cancel(Guid id, Guid eventId, DateTime originalStartUtc, bool thisAndFollowing = false)
    {
        return new EventOccurrenceException(id, eventId, originalStartUtc, true, null, null, thisAndFollowing);
    }

    public static EventOccurrenceException Move(Guid id, Guid eventId, DateTime originalStartUtc, DateTime overrideStartUtc, DateTime overrideEndUtc, bool thisAndFollowing = false)
    {
        return new EventOccurrenceException(id, eventId, originalStartUtc, false, overrideStartUtc, overrideEndUtc, thisAndFollowing);
    }

    private void Validate()
    {
        if (IsCancelled && (OverrideStartUtc.HasValue || OverrideEndUtc.HasValue))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOccurrenceOverride);
        }

        if (!IsCancelled && (!OverrideStartUtc.HasValue || !OverrideEndUtc.HasValue || OverrideEndUtc <= OverrideStartUtc))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOccurrenceOverride);
        }
    }
}

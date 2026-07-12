using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiAbp.Calendar.Events;

public class EventReminder : Entity<Guid>
{
    public virtual Guid EventId { get; private set; }

    public virtual TimeSpan Offset { get; private set; }

    public virtual ReminderChannel Channel { get; private set; }

    public virtual Guid? AttendeeId { get; private set; }

    public virtual DateTime? SentAtUtc { get; private set; }

    protected EventReminder()
    {
    }

    public EventReminder(Guid id, Guid eventId, TimeSpan offset, ReminderChannel channel, Guid? attendeeId = null)
        : base(id)
    {
        EventId = eventId;
        SetOffset(offset);
        Channel = channel;
        AttendeeId = attendeeId;
    }

    public virtual void SetOffset(TimeSpan offset)
    {
        if (offset > TimeSpan.Zero || offset < TimeSpan.FromDays(-EventConsts.MaxReminderOffsetDays))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidReminderOffset);
        }

        Offset = offset;
    }

    public virtual void MarkSent(DateTime sentAtUtc)
    {
        SentAtUtc = DateTime.SpecifyKind(sentAtUtc, DateTimeKind.Utc);
    }

    public virtual void ResetDispatch()
    {
        SentAtUtc = null;
    }
}

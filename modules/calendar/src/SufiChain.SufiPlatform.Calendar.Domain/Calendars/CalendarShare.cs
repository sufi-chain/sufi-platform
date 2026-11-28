using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

/// <summary>
/// A user granted read-only access to one calendar. The grant does not confer edit.
/// </summary>
public class CalendarShare : CreationAuditedAggregateRoot<Guid>
{
    public virtual Guid CalendarId { get; private set; }

    public virtual Guid UserId { get; private set; }

    protected CalendarShare()
    {
    }

    public CalendarShare(Guid id, Guid calendarId, Guid userId)
        : base(id)
    {
        CalendarId = calendarId;
        UserId = userId;
    }
}

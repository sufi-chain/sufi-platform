using Volo.Abp.Domain.Entities;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarInheritance : Entity<Guid>
{
    public virtual Guid CalendarId { get; private set; }

    public virtual Guid ParentCalendarId { get; private set; }

    /// <summary>
    /// When enabled, the parent calendar's working-hour rules participate in the
    /// child's availability. Parent events and exceptions are inherited regardless.
    /// </summary>
    public virtual bool IsInheritedByDefault { get; private set; }

    protected CalendarInheritance()
    {
    }

    public CalendarInheritance(Guid id, Guid calendarId, Guid parentCalendarId, bool isInheritedByDefault = false)
        : base(id)
    {
        CalendarId = calendarId;
        ParentCalendarId = parentCalendarId;
        IsInheritedByDefault = isInheritedByDefault;
    }

    public virtual void SetInheritedByDefault(bool isInheritedByDefault)
    {
        IsInheritedByDefault = isInheritedByDefault;
    }
}

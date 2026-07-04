using SufiChain.SufiAbp.Domain.Entities;
using SufiChain.SufiAbp;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class WorkingHourRule : Entity<Guid>
{
    public virtual Guid CalendarId { get; private set; }

    public virtual DayOfWeek DayOfWeek { get; private set; }

    public virtual TimeOnly StartTime { get; private set; }

    public virtual TimeOnly EndTime { get; private set; }

    public virtual int DisplayOrder { get; private set; }

    protected WorkingHourRule()
    {
    }

    public WorkingHourRule(Guid id, Guid calendarId, DayOfWeek dayOfWeek, TimeOnly startTime, TimeOnly endTime, int displayOrder = 0)
        : base(id)
    {
        CalendarId = calendarId;
        DayOfWeek = dayOfWeek;
        SetRange(startTime, endTime);
        SetDisplayOrder(displayOrder);
    }

    public virtual void SetDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
    }

    public virtual void SetRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidTimeRange);
        }

        StartTime = startTime;
        EndTime = endTime;
    }
}

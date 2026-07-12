using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class WorkingHourRuleDto : EntityDto<Guid>
{
    public Guid CalendarId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

}

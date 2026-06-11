using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class WorkingHourRuleDto : EntityDto<Guid>
{
    public Guid CalendarId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int? MaxConcurrent { get; set; }
}

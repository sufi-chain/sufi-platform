using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarInheritanceDto : EntityDto<Guid>
{
    public Guid CalendarId { get; set; }

    public Guid ParentCalendarId { get; set; }

    public string? ParentCalendarName { get; set; }

    public CalendarKind? ParentCalendarKind { get; set; }

    public bool IsInheritedByDefault { get; set; }
}

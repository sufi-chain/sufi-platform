using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarShareDto : EntityDto<Guid>
{
    public Guid CalendarId { get; set; }

    public Guid UserId { get; set; }
}

public class GrantCalendarShareInput
{
    public Guid UserId { get; set; }
}

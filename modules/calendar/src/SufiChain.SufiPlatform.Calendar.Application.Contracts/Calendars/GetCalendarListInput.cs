using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class GetCalendarListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public CalendarKind? Kind { get; set; }

    public Guid? OwnerUserId { get; set; }
}

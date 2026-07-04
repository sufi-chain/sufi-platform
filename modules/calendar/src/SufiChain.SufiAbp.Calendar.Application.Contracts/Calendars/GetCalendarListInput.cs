using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class GetCalendarListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }

    public CalendarKind? Kind { get; set; }

    public Guid? OwnerUserId { get; set; }
}

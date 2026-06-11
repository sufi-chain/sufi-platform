using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class CalendarExceptionDto : EntityDto<Guid>
{
    public Guid CalendarId { get; set; }

    public DateTime Date { get; set; }

    public CalendarExceptionKind Kind { get; set; }

    public string? Description { get; set; }

    public List<WorkingHourRangeDto> Ranges { get; set; } = new();
}

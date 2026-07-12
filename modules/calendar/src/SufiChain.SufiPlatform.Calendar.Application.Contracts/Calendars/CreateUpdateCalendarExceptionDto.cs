using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CreateUpdateCalendarExceptionDto
{
    public DateTime Date { get; set; }

    public CalendarExceptionKind Kind { get; set; }

    [StringLength(CalendarConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    public List<WorkingHourRangeDto> Ranges { get; set; } = new();
}

using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Calendar.Events;

public class GetEventListInput : PagedAndSortedResultRequestDto
{
    public Guid? CalendarId { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public string? SourceType { get; set; }

    public string? SourceId { get; set; }
}

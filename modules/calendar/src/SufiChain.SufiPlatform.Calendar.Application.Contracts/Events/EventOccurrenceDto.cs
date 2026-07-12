namespace SufiChain.SufiPlatform.Calendar.Events;

public class EventOccurrenceDto
{
    public Guid EventId { get; set; }

    public Guid CalendarId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime OriginalStartUtc { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public bool IsAllDay { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public EventStatus Status { get; set; }

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? Color { get; set; }

    public string? SourceType { get; set; }

    public string? SourceId { get; set; }
}

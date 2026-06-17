using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.AI.Tools;

public class CalendarAIFreeBusyInput
{
    public List<Guid> CalendarIds { get; set; } = new();

    public DateTime FromUtc { get; set; }

    public DateTime ToUtc { get; set; }
}

public class CalendarAIFindFreeSlotsInput : CalendarAIFreeBusyInput
{
    public TimeSpan Duration { get; set; }
}

public class CalendarAICreateEventInput
{
    public Guid CalendarId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public bool IsAllDay { get; set; }

    public string TimeZoneId { get; set; } = "UTC";

    public string? Location { get; set; }

    public string? Description { get; set; }

    public Guid? AvailabilityCalendarId { get; set; }

    public string? SourceType { get; set; }

    public string? SourceId { get; set; }
}

public class CalendarAISearchEventsInput
{
    public Guid? CalendarId { get; set; }

    public DateTime? FromUtc { get; set; }

    public DateTime? ToUtc { get; set; }

    public string? TitleContains { get; set; }

    public int MaxResultCount { get; set; } = 10;
}

public class CalendarAIMoveEventInput
{
    public Guid EventId { get; set; }

    public DateTime MovedStartUtc { get; set; }

    public DateTime MovedEndUtc { get; set; }
}

public class CalendarAICancelEventInput
{
    public Guid EventId { get; set; }
}

public class CalendarAIMoveOccurrenceInput
{
    public Guid EventId { get; set; }

    public DateTime OriginalStartUtc { get; set; }

    public DateTime MovedStartUtc { get; set; }

    public DateTime MovedEndUtc { get; set; }

    public bool ThisAndFollowing { get; set; }
}

public class CalendarAICancelOccurrenceInput
{
    public Guid EventId { get; set; }

    public DateTime OriginalStartUtc { get; set; }

    public bool ThisAndFollowing { get; set; }
}

public class CalendarAITestAvailabilityInput
{
    public Guid CalendarId { get; set; }

    public DateTime UtcInstant { get; set; }
}

public class CalendarAIListCalendarsInput
{
    public string? Filter { get; set; }
}

public class CalendarAIGetWorkingHoursInput
{
    public Guid CalendarId { get; set; }
}

public class CalendarAIGetCurrentTimeInput
{
    public Guid? CalendarId { get; set; }

    public string? TimeZoneId { get; set; }
}

public class CalendarAIEventResult
{
    public Guid Id { get; set; }

    public Guid CalendarId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Location { get; set; }

    public static CalendarAIEventResult From(CalendarEventDto dto)
    {
        return new CalendarAIEventResult
        {
            Id = dto.Id,
            CalendarId = dto.CalendarId,
            Title = dto.Title,
            StartUtc = dto.StartUtc,
            EndUtc = dto.EndUtc,
            Status = dto.Status.ToString(),
            Location = dto.Location
        };
    }
}

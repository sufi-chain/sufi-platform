namespace SufiChain.SufiPlatform.Calendar.Events;

public sealed record EventOccurrence(
    Guid EventId,
    Guid CalendarId,
    string Title,
    DateTime OriginalStartUtc,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsAllDay,
    string TimeZoneId,
    EventStatus Status,
    string? Location,
    string? Description,
    string? Color,
    string? SourceType,
    string? SourceId);

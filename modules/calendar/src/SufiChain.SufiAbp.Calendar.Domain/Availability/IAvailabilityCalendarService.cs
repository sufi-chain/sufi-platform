namespace SufiChain.SufiAbp.Calendar.Availability;

public interface IAvailabilityCalendarService
{
    Task<bool> IsOpenAtAsync(Guid calendarId, DateTime utcInstant, CancellationToken ct = default);

    Task<DateTime> NextOpenAtAsync(Guid calendarId, DateTime utcFrom, CancellationToken ct = default);

    Task<DateTime> NextCloseAtAsync(Guid calendarId, DateTime utcFrom, CancellationToken ct = default);

    Task<DateTime> AddWorkingDurationAsync(Guid calendarId, DateTime utcStart, TimeSpan working, CancellationToken ct = default);

    Task<TimeSpan> ElapsedWorkingTimeAsync(Guid calendarId, DateTime utcFrom, DateTime utcTo, CancellationToken ct = default);
}

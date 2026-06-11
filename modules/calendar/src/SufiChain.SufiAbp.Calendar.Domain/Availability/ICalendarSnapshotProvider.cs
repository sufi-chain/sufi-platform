namespace SufiChain.SufiAbp.Calendar.Availability;

public interface ICalendarSnapshotProvider
{
    Task<CalendarSnapshot> GetAsync(Guid calendarId, CancellationToken cancellationToken = default);
}

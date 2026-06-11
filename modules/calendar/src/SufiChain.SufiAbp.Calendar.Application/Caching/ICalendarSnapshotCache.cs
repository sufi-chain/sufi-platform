using SufiChain.SufiAbp.Calendar.Availability;

namespace SufiChain.SufiAbp.Calendar.Caching;

public interface ICalendarSnapshotCache : ICalendarSnapshotProvider
{
    Task<CalendarSnapshot> GetAsync(Guid calendarId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default);
}

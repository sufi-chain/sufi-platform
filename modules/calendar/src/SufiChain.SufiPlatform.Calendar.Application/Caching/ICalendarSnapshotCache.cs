using SufiChain.SufiPlatform.Calendar.Availability;

namespace SufiChain.SufiPlatform.Calendar.Caching;

public interface ICalendarSnapshotCache : ICalendarSnapshotProvider
{
    Task<CalendarSnapshot> GetAsync(Guid calendarId, CancellationToken cancellationToken = default);

    Task RemoveAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default);

    Task RemoveWithInheritorsAsync(Guid calendarId, Guid? tenantId = null, CancellationToken cancellationToken = default);
}

using SufiChain.SufiAbp.Domain.Repositories;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public interface ICalendarRepository : IRepository<Calendar, Guid>
{
    Task<Calendar?> FindDefaultAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken = default);

   Task<List<Calendar>> GetInheritedCalendarsAsync(Guid calendarId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetInheritedCalendarIdsAsync(Guid calendarId, CancellationToken cancellationToken = default);

    Task<List<Guid>> GetInheritingCalendarIdsAsync(Guid parentCalendarId, CancellationToken cancellationToken = default);

    Task<List<Calendar>> GetByOwnerUserIdAsync(Guid? tenantId, Guid ownerUserId, CancellationToken cancellationToken = default);

    Task<Calendar?> FindByNameAsync(Guid? tenantId, string name, CancellationToken cancellationToken = default);
}

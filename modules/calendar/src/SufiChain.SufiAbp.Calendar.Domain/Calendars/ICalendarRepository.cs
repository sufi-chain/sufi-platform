using SufiChain.SufiAbp.Domain.Repositories;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public interface ICalendarRepository : IRepository<Calendar, Guid>
{
    Task<Calendar?> FindDefaultAsync(Guid? tenantId, CalendarKind kind, CancellationToken cancellationToken = default);
}

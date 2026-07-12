using SufiChain.SufiAbp.Calendar.Reminders;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.Calendar.Events;

public interface ICalendarEventRepository : IRepository<CalendarEvent, Guid>
{
    Task<List<CalendarEvent>> GetListInWindowAsync(Guid calendarId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default);

    Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, CancellationToken cancellationToken = default);

    Task<List<CalendarEvent>> GetListBySourceAsync(string sourceType, string sourceId, CancellationToken cancellationToken = default);
}

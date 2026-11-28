using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Reminders;
using SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore.Repositories;

public class EfCoreCalendarEventRepository : EfCoreRepository<ICalendarDbContext, CalendarEvent, Guid>, ICalendarEventRepository
{
    public EfCoreCalendarEventRepository(IDbContextProvider<ICalendarDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<CalendarEvent>> GetListInWindowAsync(Guid calendarId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        // Expansion only needs recurrence + occurrence overrides — not attendees/reminders.
        return await dbSet
            .Include(x => x.RecurrenceRule)
            .Include(x => x.OccurrenceExceptions)
            .Where(CalendarEventWindowQuery.MatchesWindow(calendarId, fromUtc, toUtc))
            .ToListAsync(cancellationToken);
    }

    public override async Task<IQueryable<CalendarEvent>> WithDetailsAsync()
    {
        return (await GetQueryableAsync())
            .Include(x => x.RecurrenceRule)
            .Include(x => x.OccurrenceExceptions)
            .Include(x => x.Attendees)
            .Include(x => x.Reminders);
    }

    public virtual async Task<List<CalendarEvent>> GetListBySourceAsync(string sourceType, string sourceId, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.RecurrenceRule)
            .Include(x => x.OccurrenceExceptions)
            .Include(x => x.Attendees)
            .Include(x => x.Reminders)
            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
            .ToListAsync(cancellationToken);
    }

    public virtual Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        return GetDueRemindersAsync(nowUtc, TimeSpan.FromMinutes(CalendarReminderSettings.DefaultCatchUpMinutes), cancellationToken);
    }

    public virtual async Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, TimeSpan catchUp, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var events = await dbSet
            .Include(x => x.RecurrenceRule)
            .Include(x => x.OccurrenceExceptions)
            .Include(x => x.Attendees)
            .Include(x => x.Reminders)
            .Where(x => x.Reminders.Any(r => r.SentAtUtc == null))
            .ToListAsync(cancellationToken);

        return ReminderDispatchPlanner.Select(events, nowUtc, catchUp);
    }
}

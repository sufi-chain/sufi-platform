using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Reminders;
using SufiChain.SufiPlatform.Calendar.MongoDB;
using Volo.Abp.Domain.Repositories.MongoDB;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Calendar.MongoDB.Repositories;

public class MongoCalendarEventRepository : MongoDbRepository<ICalendarMongoDbContext, CalendarEvent, Guid>, ICalendarEventRepository
{
    public MongoCalendarEventRepository(IMongoDbContextProvider<ICalendarMongoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<CalendarEvent>> GetListInWindowAsync(Guid calendarId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(CalendarEventWindowQuery.MatchesWindow(calendarId, fromUtc, toUtc))
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<CalendarEvent>> GetListBySourceAsync(string sourceType, string sourceId, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
            .ToListAsync(cancellationToken);
    }

    public virtual Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        return GetDueRemindersAsync(nowUtc, TimeSpan.FromMinutes(CalendarReminderSettings.DefaultCatchUpMinutes), cancellationToken);
    }

    public virtual async Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, TimeSpan catchUp, CancellationToken cancellationToken = default)
    {
        var events = await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.Reminders.Any(r => r.SentAtUtc == null))
            .ToListAsync(cancellationToken);

        return ReminderDispatchPlanner.Select(events, nowUtc, catchUp);
    }
}

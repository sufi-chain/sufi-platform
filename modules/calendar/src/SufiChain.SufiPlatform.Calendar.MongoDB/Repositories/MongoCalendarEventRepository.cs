using MongoDB.Driver.Linq;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Reminders;
using SufiChain.SufiPlatform.Calendar.Scheduling;
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
            .Where(x => x.CalendarId == calendarId && (x.RecurrenceRule != null || (x.StartUtc < toUtc && x.EndUtc > fromUtc)))
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<CalendarEvent>> GetListBySourceAsync(string sourceType, string sourceId, CancellationToken cancellationToken = default)
    {
        return await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.SourceType == sourceType && x.SourceId == sourceId)
            .ToListAsync(cancellationToken);
    }

    public virtual async Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var events = await (await GetQueryableAsync(cancellationToken))
            .Where(x => x.Reminders.Any(r => r.SentAtUtc == null))
            .ToListAsync(cancellationToken);

        var calculator = new RecurrenceCalculator();
        var items = new List<EventReminderDispatchItem>();
        foreach (var calendarEvent in events)
        {
            foreach (var reminder in calendarEvent.Reminders.Where(x => x.SentAtUtc == null))
            {
                var targetStartUtc = nowUtc.Subtract(reminder.Offset);
                var from = targetStartUtc.AddMinutes(-1);
                var to = targetStartUtc.AddMinutes(1);
                foreach (var occurrence in calculator.Expand(calendarEvent, from, to))
                {
                    var dueAtUtc = occurrence.StartUtc.Add(reminder.Offset);
                    if (dueAtUtc <= nowUtc)
                    {
                        var attendee = reminder.AttendeeId.HasValue
                            ? calendarEvent.Attendees.FirstOrDefault(x => x.Id == reminder.AttendeeId.Value)
                            : calendarEvent.Attendees.FirstOrDefault(x => x.Role == AttendeeRole.Organizer) ?? calendarEvent.Attendees.FirstOrDefault();
                        items.Add(new EventReminderDispatchItem(calendarEvent, reminder, occurrence, attendee, dueAtUtc));
                    }
                }
            }
        }

        return items;
    }
}

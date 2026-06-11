using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiAbp.Calendar.Reminders;
using SufiChain.SufiAbp.Calendar.Scheduling;
using SufiChain.SufiAbp.Calendar.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Calendar.EntityFrameworkCore.Repositories;

public class EfCoreCalendarEventRepository : EfCoreRepository<ICalendarDbContext, CalendarEvent, Guid>, ICalendarEventRepository
{
    public EfCoreCalendarEventRepository(IDbContextProvider<ICalendarDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public virtual async Task<List<CalendarEvent>> GetListInWindowAsync(Guid calendarId, DateTime fromUtc, DateTime toUtc, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(x => x.RecurrenceRule)
            .Include(x => x.OccurrenceExceptions)
            .Include(x => x.Attendees)
            .Include(x => x.Reminders)
            .Where(x => x.CalendarId == calendarId && (x.RecurrenceRule != null || (x.StartUtc < toUtc && x.EndUtc > fromUtc)))
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

    public virtual async Task<List<EventReminderDispatchItem>> GetDueRemindersAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var dbSet = await GetDbSetAsync();
        var events = await dbSet
            .Include(x => x.RecurrenceRule)
            .Include(x => x.OccurrenceExceptions)
            .Include(x => x.Attendees)
            .Include(x => x.Reminders)
            .Where(x => x.Reminders.Any(r => r.SentAtUtc == null))
            .ToListAsync(cancellationToken);

        return BuildDueReminderItems(events, nowUtc);
    }

    private static List<EventReminderDispatchItem> BuildDueReminderItems(IEnumerable<CalendarEvent> events, DateTime nowUtc)
    {
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

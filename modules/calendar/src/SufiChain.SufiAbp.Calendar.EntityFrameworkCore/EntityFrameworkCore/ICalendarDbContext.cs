using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Calendar.EntityFrameworkCore;

[ConnectionStringName(CalendarConsts.ConnectionStringName)]
public interface ICalendarDbContext : IEfCoreDbContext
{
    DbSet<Calendars.Calendar> Calendars { get; }

    DbSet<WorkingHourRule> WorkingHourRules { get; }

    DbSet<CalendarException> CalendarExceptions { get; }

    DbSet<CalendarEvent> CalendarEvents { get; }

    DbSet<EventOccurrenceException> EventOccurrenceExceptions { get; }

    DbSet<EventAttendee> EventAttendees { get; }

    DbSet<EventReminder> EventReminders { get; }
}

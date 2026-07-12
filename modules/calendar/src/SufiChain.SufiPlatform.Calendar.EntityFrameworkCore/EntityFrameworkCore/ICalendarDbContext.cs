using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.Calendar;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

[ConnectionStringName(SufiCalendarDbProperties.ConnectionStringName)]
public interface ICalendarDbContext : IEfCoreDbContext
{
    DbSet<Calendars.Calendar> Calendars { get; }

    DbSet<WorkingHourRule> WorkingHourRules { get; }

    DbSet<CalendarException> CalendarExceptions { get; }

    DbSet<CalendarInheritance> CalendarInheritances { get; }

    DbSet<CalendarEvent> CalendarEvents { get; }

    DbSet<EventOccurrenceException> EventOccurrenceExceptions { get; }

    DbSet<EventAttendee> EventAttendees { get; }

    DbSet<EventReminder> EventReminders { get; }
}
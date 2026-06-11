using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.Calendar.EntityFrameworkCore;

[ConnectionStringName(CalendarConsts.ConnectionStringName)]
public class CalendarDbContext : AbpDbContext<CalendarDbContext>, ICalendarDbContext
{
    public DbSet<Calendars.Calendar> Calendars { get; set; } = null!;

    public DbSet<WorkingHourRule> WorkingHourRules { get; set; } = null!;

    public DbSet<CalendarException> CalendarExceptions { get; set; } = null!;

    public DbSet<CalendarEvent> CalendarEvents { get; set; } = null!;

    public DbSet<EventOccurrenceException> EventOccurrenceExceptions { get; set; } = null!;

    public DbSet<EventAttendee> EventAttendees { get; set; } = null!;

    public DbSet<EventReminder> EventReminders { get; set; } = null!;

    public CalendarDbContext(DbContextOptions<CalendarDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ConfigureSufiAbpCalendar();
    }
}

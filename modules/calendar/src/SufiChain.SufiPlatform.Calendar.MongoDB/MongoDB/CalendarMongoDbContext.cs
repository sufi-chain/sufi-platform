using MongoDB.Driver;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.Calendar;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

[ConnectionStringName(SufiCalendarDbProperties.ConnectionStringName)]
public class CalendarMongoDbContext : AbpMongoDbContext, ICalendarMongoDbContext
{
    public IMongoCollection<Calendars.Calendar> Calendars => Collection<Calendars.Calendar>();

    public IMongoCollection<CalendarEvent> CalendarEvents => Collection<CalendarEvent>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.ConfigureSufiCalendar();
    }
}
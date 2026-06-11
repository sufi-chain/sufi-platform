using MongoDB.Driver;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Calendar.MongoDB;

[ConnectionStringName(CalendarConsts.ConnectionStringName)]
public class CalendarMongoDbContext : AbpMongoDbContext, ICalendarMongoDbContext
{
    public IMongoCollection<Calendars.Calendar> Calendars => Collection<Calendars.Calendar>();

    public IMongoCollection<CalendarEvent> CalendarEvents => Collection<CalendarEvent>();

    protected override void CreateModel(IMongoModelBuilder modelBuilder)
    {
        base.CreateModel(modelBuilder);
        modelBuilder.ConfigureSufiAbpCalendar();
    }
}

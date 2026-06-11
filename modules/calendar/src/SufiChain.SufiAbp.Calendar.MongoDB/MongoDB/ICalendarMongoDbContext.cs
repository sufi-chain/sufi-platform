using MongoDB.Driver;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Calendar.MongoDB;

[ConnectionStringName(CalendarConsts.ConnectionStringName)]
public interface ICalendarMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Calendars.Calendar> Calendars { get; }

    IMongoCollection<CalendarEvent> CalendarEvents { get; }
}

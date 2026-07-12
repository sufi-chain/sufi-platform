using MongoDB.Driver;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.MongoDB;
using SufiChain.SufiPlatform.Calendar;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

[ConnectionStringName(SufiCalendarDbProperties.ConnectionStringName)]
public interface ICalendarMongoDbContext : IAbpMongoDbContext
{
    IMongoCollection<Calendars.Calendar> Calendars { get; }

    IMongoCollection<CalendarEvent> CalendarEvents { get; }
}
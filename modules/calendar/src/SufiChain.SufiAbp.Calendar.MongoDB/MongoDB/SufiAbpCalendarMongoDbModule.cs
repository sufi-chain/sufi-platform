using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiAbp.Calendar.MongoDB.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Calendar.MongoDB;

[DependsOn(
    typeof(SufiAbpCalendarDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiAbpCalendarMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<CalendarMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<ICalendarMongoDbContext>();
            options.AddRepository<Calendars.Calendar, MongoCalendarRepository>();
            options.AddRepository<CalendarEvent, MongoCalendarEventRepository>();
        });

        context.Services.AddTransient<ICalendarRepository, MongoCalendarRepository>();
        context.Services.AddTransient<ICalendarEventRepository, MongoCalendarEventRepository>();
    }
}

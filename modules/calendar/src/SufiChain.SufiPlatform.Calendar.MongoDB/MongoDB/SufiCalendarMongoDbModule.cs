using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.MongoDB.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiPlatform.Calendar.MongoDB;

[DependsOn(
    typeof(SufiCalendarDomainModule),
    typeof(AbpMongoDbModule)
)]
public class SufiCalendarMongoDbModule : AbpModule
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
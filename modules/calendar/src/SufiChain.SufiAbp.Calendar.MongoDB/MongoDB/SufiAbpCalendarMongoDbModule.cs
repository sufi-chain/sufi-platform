using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.MongoDB.Repositories;
using SufiChain.SufiAbp.MongoDB;
using Volo.Abp.Modularity;
using Volo.Abp.MongoDB;

namespace SufiChain.SufiAbp.Calendar.MongoDB;

[DependsOn(
    typeof(SufiAbpCalendarDomainModule),
    typeof(SufiAbpMongoDbModule)
)]
public class SufiAbpCalendarMongoDbModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddMongoDbContext<CalendarMongoDbContext>(options =>
        {
            options.AddDefaultRepositories<ICalendarMongoDbContext>();
            options.AddRepository<Calendars.Calendar, MongoCalendarRepository>();
        });
    }
}

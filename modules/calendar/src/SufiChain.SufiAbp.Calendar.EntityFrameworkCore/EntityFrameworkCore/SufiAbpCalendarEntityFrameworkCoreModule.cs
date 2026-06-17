using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.EntityFrameworkCore.Repositories;
using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiAbp.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar.EntityFrameworkCore;

[DependsOn(
    typeof(SufiAbpCalendarDomainModule),
    typeof(SufiAbpEntityFrameworkCoreModule)
)]
public class SufiAbpCalendarEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<CalendarDbContext>(options =>
        {
            options.AddDefaultRepositories<ICalendarDbContext>();
            options.AddRepository<Calendars.Calendar, EfCoreCalendarRepository>();
            options.AddRepository<CalendarEvent, EfCoreCalendarEventRepository>();
        });

        context.Services.AddTransient<ICalendarRepository, EfCoreCalendarRepository>();
        context.Services.AddTransient<ICalendarEventRepository, EfCoreCalendarEventRepository>();

        Configure<AbpDbConnectionOptions>(options =>
        {
            options.Databases.Configure(CalendarConsts.ConnectionStringName, database =>
            {
                database.IsUsedByTenants = true;
            });
        });
    }
}

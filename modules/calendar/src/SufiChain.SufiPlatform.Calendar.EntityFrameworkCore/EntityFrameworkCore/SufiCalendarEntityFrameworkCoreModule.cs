using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.EntityFrameworkCore.Repositories;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;
using SufiChain.SufiPlatform.Calendar;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

[DependsOn(
    typeof(SufiCalendarDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class SufiCalendarEntityFrameworkCoreModule : AbpModule
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
            options.Databases.Configure(SufiCalendarDbProperties.ConnectionStringName, database =>
            {
                database.IsUsedByTenants = true;
            });
        });
    }
}
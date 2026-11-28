using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Caching;
using SufiChain.SufiPlatform.Calendar.Reminders;
using SufiChain.SufiPlatform.Calendar.Scheduling;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Modularity;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(SufiCalendarDomainModule),
    typeof(SufiCalendarApplicationContractsModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpCachingModule),
    typeof(AbpBackgroundWorkersModule)
)]
public class SufiCalendarApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICalendarSnapshotCache, CalendarSnapshotCache>();
        context.Services.AddTransient<ICalendarSnapshotProvider, CalendarSnapshotCache>();
        context.Services.Replace(
            ServiceDescriptor.Transient<ICalendarOccurrenceExpansionCache, CalendarOccurrenceExpansionCache>());
    }

    public override async Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
    {
        await context.AddBackgroundWorkerAsync<CalendarReminderDispatchWorker>();
    }
}

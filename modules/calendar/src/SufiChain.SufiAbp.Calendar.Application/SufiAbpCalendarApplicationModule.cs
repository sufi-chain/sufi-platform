using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Caching;
using SufiChain.SufiAbp.Ddd;
using Volo.Abp.Modularity;

using Volo.Abp.Caching;
namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpCalendarDomainModule),
    typeof(SufiAbpCalendarApplicationContractsModule),
    typeof(SufiAbpDddApplicationModule),
    typeof(AbpCachingModule)
)]
public class SufiAbpCalendarApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICalendarSnapshotCache, CalendarSnapshotCache>();
        context.Services.AddTransient<ICalendarSnapshotProvider, CalendarSnapshotCache>();
    }
}

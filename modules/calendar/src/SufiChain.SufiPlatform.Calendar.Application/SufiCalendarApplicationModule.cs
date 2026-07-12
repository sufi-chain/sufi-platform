using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Caching;
using SufiChain.SufiPlatform.Ddd;
using Volo.Abp.Modularity;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(SufiCalendarDomainModule),
    typeof(SufiCalendarApplicationContractsModule),
    typeof(SufiDddApplicationModule),
    typeof(AbpCachingModule)
)]
public class SufiCalendarApplicationModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddTransient<ICalendarSnapshotCache, CalendarSnapshotCache>();
        context.Services.AddTransient<ICalendarSnapshotProvider, CalendarSnapshotCache>();
    }
}
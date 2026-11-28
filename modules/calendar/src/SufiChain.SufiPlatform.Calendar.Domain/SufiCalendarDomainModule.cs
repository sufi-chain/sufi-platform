using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Users;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Modularity;
using Volo.Abp.Domain;

using Volo.Abp.Caching;
namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(AbpCachingModule),
    typeof(SufiUsersAbstractionModule),
    typeof(SufiCalendarDomainSharedModule)
)]
public class SufiCalendarDomainModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpAuditingOptions>(options =>
        {
            options.IsEnabled = true;
            options.EntityHistorySelectors.Add(
                new NamedTypeSelector(
                    "SufiCalendar.CalendarEvent",
                    type => type == typeof(CalendarEvent)));
        });
    }
}
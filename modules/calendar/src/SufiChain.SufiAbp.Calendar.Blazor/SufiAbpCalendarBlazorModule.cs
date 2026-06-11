using SufiChain.SufiAbp.Calendar.Blazor.Menus;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(typeof(SufiAbpCalendarApplicationContractsModule))]
public class SufiAbpCalendarBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new CalendarMenuContributor());
        });

        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpCalendarBlazorModule).Assembly);
        });
    }
}

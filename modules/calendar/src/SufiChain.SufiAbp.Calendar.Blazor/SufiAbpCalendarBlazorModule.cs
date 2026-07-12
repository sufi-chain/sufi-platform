using SufiChain.SufiAbp.Calendar.Blazor.Menus;
using SufiChain.SufiAbp.Calendar.Blazor.Public;
using SufiChain.SufiAbp.UI.Bundling;
using SufiChain.SufiAbp.UI.Navigation;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar;

[DependsOn(
    typeof(SufiAbpCalendarApplicationContractsModule),
    typeof(SufiAbpCalendarBlazorPublicModule))]
public class SufiAbpCalendarBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(
                CalendarBundles.Styles.Global,
                "/_content/SufiChain.SufiAbp.Calendar.Blazor/calendar.css");
        });

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

public static class CalendarBundles
{
    public static class Styles
    {
        public const string Global = "Blazor.SufiTheme.SufiBlazor.Global";
    }
}

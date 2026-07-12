using SufiChain.SufiPlatform.Calendar.Blazor.Menus;
using SufiChain.SufiPlatform.Calendar.Blazor.Public;
using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar;

[DependsOn(
    typeof(SufiCalendarApplicationContractsModule),
    typeof(SufiCalendarBlazorPublicModule))]
public class SufiCalendarBlazorModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(
                CalendarBundles.Styles.Global,
                "/_content/SufiChain.SufiPlatform.Calendar.Blazor/calendar.css");
        });

        Configure<SufiNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new CalendarMenuContributor());
        });

        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiCalendarBlazorModule).Assembly);
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
using SufiChain.SufiAbp.UI.Bundling;
using SufiChain.SufiAbp.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public;

[DependsOn(typeof(SufiAbpCalendarApplicationContractsModule))]
public class SufiAbpCalendarBlazorPublicModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(
                CalendarPublicBundles.Styles.Global,
                "/_content/SufiChain.SufiAbp.Calendar.Blazor.Public/calendar-public.css");
        });

        Configure<SufiAbpRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiAbpCalendarBlazorPublicModule).Assembly);
        });
    }
}

public static class CalendarPublicBundles
{
    public static class Styles
    {
        public const string Global = "Blazor.KomTheme.SufiBlazor.Global";
    }
}

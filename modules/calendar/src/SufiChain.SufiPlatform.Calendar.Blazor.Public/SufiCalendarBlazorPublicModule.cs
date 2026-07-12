using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public;

[DependsOn(typeof(SufiCalendarApplicationContractsModule))]
public class SufiCalendarBlazorPublicModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(
                CalendarPublicBundles.Styles.Global,
                "/_content/SufiChain.SufiPlatform.Calendar.Blazor.Public/calendar-public.css");
        });

        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiCalendarBlazorPublicModule).Assembly);
        });
    }
}

public static class CalendarPublicBundles
{
    public static class Styles
    {
        public const string Global = "Blazor.SufiTheme.SufiBlazor.Global";
    }
}
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Blazor.Public;
using SufiChain.SufiPlatform.Calendar.Blazor.Public;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.Menus;
using SufiChain.SufiPlatform.UI.Bundling;
using SufiChain.SufiPlatform.UI.Navigation;
using SufiChain.SufiPlatform.UI.Routing;
using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor;

[DependsOn(
    typeof(SufiCalendarBlazorPublicModule),
    typeof(SufiAIHooshvareBlazorPublicModule)
)]
public class SufiCalendarAssistantBlazorModule : AbpModule
{
    public const string StylePath = "/_content/SufiChain.SufiPlatform.Calendar.Assistant.Blazor/calendar-hooshvare.css";

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<BundleOptions>(options =>
        {
            options.StyleBundles.Add(CalendarPublicBundles.Styles.Global, StylePath);
        });

        Configure<SufiRouterOptions>(options =>
        {
            options.AdditionalAssemblies.Add(typeof(SufiCalendarAssistantBlazorModule).Assembly);
        });
    }

    public override void PostConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiNavigationOptions>(options =>
        {
            if (options.MenuContributors.Any(contributor => contributor is CalendarHooshvareMenuContributor))
            {
                return;
            }

            options.MenuContributors.Add(new CalendarHooshvareMenuContributor());
        });
    }
}

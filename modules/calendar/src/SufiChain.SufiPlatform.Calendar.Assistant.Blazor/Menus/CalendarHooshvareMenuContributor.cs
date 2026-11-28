using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Calendar.Features;
using SufiChain.SufiPlatform.UI.Navigation;
using Volo.Abp.Features;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.Menus;

/// <summary>
/// Aligns the portal calendar menu URL with the Hooshvare page when the host redirects
/// /panel/portal/calendar → /panel/portal/calendar-pro.
/// </summary>
public class CalendarHooshvareMenuContributor : IMenuContributor
{
    public const string PortalCalendarMenuName = "SufiCalendar.Portal";
    public const string PortalCalendarProUrl = "/panel/portal/calendar-pro";

    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return;
        }

        var features = context.ServiceProvider.GetRequiredService<IFeatureChecker>();
        if (!CalendarAssistantChrome.RewritesPortalRoute(
                await features.IsEnabledAsync(CalendarFeatures.Assistant)))
        {
            return;
        }

        var portalCalendar = context.Menu.GetMenuItem(PortalCalendarMenuName);
        if (portalCalendar != null)
        {
            portalCalendar.Url = PortalCalendarProUrl;
        }
    }
}

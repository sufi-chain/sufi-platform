using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Menus;

public class CalendarMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name != StandardMenus.Main)
        {
            return Task.CompletedTask;
        }

        var l = context.GetLocalizer<CalendarResource>();

        context.Menu.GetAdministration().AddItem(new ApplicationMenuItem(
            CalendarMenus.Calendars,
            l["Menu:SufiCalendar"],
            url: "/panel/admin/calendar",
            icon: "calendar",
            requiredPermissionName: CalendarPermissions.Calendars.Default));

        context.Menu.GetPortal().AddItem(new ApplicationMenuItem(
            CalendarMenus.PortalCalendar,
            l["Menu:SufiCalendar"],
            url: "/panel/portal/calendar",
            icon: "calendar",
            order: 25,
            requiredPermissionName: CalendarPermissions.Events.Default));

        return Task.CompletedTask;
    }
}

using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.Calendar.Permissions;
using SufiChain.SufiPlatform.UI.Navigation;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Menus;

public class CalendarMenuContributor : IMenuContributor
{
    public Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            var administrationMenu = context.Menu.GetAdministration();
            var l = context.GetLocalizer<CalendarResource>();

            administrationMenu.AddItem(new ApplicationMenuItem(
                CalendarMenus.Calendars,
                l["Menu:SufiCalendar"],
                url: "/panel/admin/calendar",
                icon: "calendar",
                requiredPermissionName: CalendarPermissions.Calendars.Default));
        }

        return Task.CompletedTask;
    }
}
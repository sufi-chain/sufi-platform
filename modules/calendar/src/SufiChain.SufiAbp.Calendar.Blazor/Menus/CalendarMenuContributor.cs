using SufiChain.SufiAbp.Calendar.Localization;
using SufiChain.SufiAbp.Calendar.Permissions;
using SufiChain.SufiAbp.UI.Navigation;

namespace SufiChain.SufiAbp.Calendar.Blazor.Menus;

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
                l["Menu:Calendar"],
                url: "/panel/admin/calendar",
                icon: "calendar",
                requiredPermissionName: CalendarPermissions.Calendars.Default));
        }

        return Task.CompletedTask;
    }
}

using SufiChain.SufiAbp.Authorization.Permissions;
using SufiChain.SufiAbp.Calendar.Localization;
using Volo.Abp.Localization;
namespace SufiChain.SufiAbp.Calendar.Permissions;

public class CalendarPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(CalendarPermissions.GroupName, L("Permission:Calendar"));
        var calendars = group.AddPermission(CalendarPermissions.Calendars.Default, L("Permission:Calendar.Calendars"));
        calendars.AddChild(CalendarPermissions.Calendars.Create, L("Permission:Create"));
        calendars.AddChild(CalendarPermissions.Calendars.Update, L("Permission:Update"));
        calendars.AddChild(CalendarPermissions.Calendars.Delete, L("Permission:Delete"));
        calendars.AddChild(CalendarPermissions.Calendars.ManageHours, L("Permission:Calendar.ManageHours"));
        calendars.AddChild(CalendarPermissions.Calendars.ManageExceptions, L("Permission:Calendar.ManageExceptions"));
        calendars.AddChild(CalendarPermissions.Calendars.Share, L("Permission:Calendar.Share"));

        var events = group.AddPermission(CalendarPermissions.Events.Default, L("Permission:Calendar.Events"));
        events.AddChild(CalendarPermissions.Events.Create, L("Permission:Create"));
        events.AddChild(CalendarPermissions.Events.Update, L("Permission:Update"));
        events.AddChild(CalendarPermissions.Events.Delete, L("Permission:Delete"));
        events.AddChild(CalendarPermissions.Events.ManageAttendees, L("Permission:Calendar.ManageAttendees"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CalendarResource>(name);
    }
}

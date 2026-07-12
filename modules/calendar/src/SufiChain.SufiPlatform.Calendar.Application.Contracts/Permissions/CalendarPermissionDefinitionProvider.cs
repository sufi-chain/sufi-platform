using SufiChain.SufiPlatform.Authorization.Permissions;
using SufiChain.SufiPlatform.Calendar.Localization;
using Volo.Abp.Localization;
namespace SufiChain.SufiPlatform.Calendar.Permissions;

public class CalendarPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(CalendarPermissions.GroupName, L("Permission:SufiCalendar"));
        var calendars = group.AddPermission(CalendarPermissions.Calendars.Default, L("Permission:SufiCalendar.Calendars"));
        calendars.AddChild(CalendarPermissions.Calendars.Create, L("Permission:Create"));
        calendars.AddChild(CalendarPermissions.Calendars.Update, L("Permission:Update"));
        calendars.AddChild(CalendarPermissions.Calendars.Delete, L("Permission:Delete"));
        calendars.AddChild(CalendarPermissions.Calendars.ManageHours, L("Permission:SufiCalendar.ManageHours"));
        calendars.AddChild(CalendarPermissions.Calendars.ManageExceptions, L("Permission:SufiCalendar.ManageExceptions"));
        calendars.AddChild(CalendarPermissions.Calendars.Share, L("Permission:SufiCalendar.Share"));

        var events = group.AddPermission(CalendarPermissions.Events.Default, L("Permission:SufiCalendar.Events"));
        events.AddChild(CalendarPermissions.Events.Create, L("Permission:Create"));
        events.AddChild(CalendarPermissions.Events.Update, L("Permission:Update"));
        events.AddChild(CalendarPermissions.Events.Delete, L("Permission:Delete"));
        events.AddChild(CalendarPermissions.Events.ManageAttendees, L("Permission:SufiCalendar.ManageAttendees"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<CalendarResource>(name);
    }
}
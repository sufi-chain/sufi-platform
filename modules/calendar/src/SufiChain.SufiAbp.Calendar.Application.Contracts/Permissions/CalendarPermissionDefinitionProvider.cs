using SufiChain.SufiAbp.Authorization.Permissions;
namespace SufiChain.SufiAbp.Calendar.Permissions;

public class CalendarPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var group = context.AddGroup(CalendarPermissions.GroupName, "Permission:Calendar");
        var calendars = group.AddPermission(CalendarPermissions.Calendars.Default, "Permission:Calendar.Calendars");
        calendars.AddChild(CalendarPermissions.Calendars.Create, "Permission:Create");
        calendars.AddChild(CalendarPermissions.Calendars.Update, "Permission:Update");
        calendars.AddChild(CalendarPermissions.Calendars.Delete, "Permission:Delete");
        calendars.AddChild(CalendarPermissions.Calendars.ManageHours, "Permission:Calendar.ManageHours");
        calendars.AddChild(CalendarPermissions.Calendars.ManageExceptions, "Permission:Calendar.ManageExceptions");
        calendars.AddChild(CalendarPermissions.Calendars.Share, "Permission:Calendar.Share");

        var events = group.AddPermission(CalendarPermissions.Events.Default, "Permission:Calendar.Events");
        events.AddChild(CalendarPermissions.Events.Create, "Permission:Create");
        events.AddChild(CalendarPermissions.Events.Update, "Permission:Update");
        events.AddChild(CalendarPermissions.Events.Delete, "Permission:Delete");
        events.AddChild(CalendarPermissions.Events.ManageAttendees, "Permission:Calendar.ManageAttendees");
    }

}
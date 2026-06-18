using Volo.Abp.Reflection;

namespace SufiChain.SufiAbp.Calendar.Permissions;

public static class CalendarPermissions
{
    public const string GroupName = "Calendar";

    public static class Calendars
    {
        public const string Default = GroupName + ".Calendars";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManageHours = Default + ".ManageHours";
        public const string ManageExceptions = Default + ".ManageExceptions";
        public const string Share = Default + ".Share";
    }

    public static class Events
    {
        public const string Default = GroupName + ".Events";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string ManageAttendees = Default + ".ManageAttendees";
    }

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(CalendarPermissions));
    }
}

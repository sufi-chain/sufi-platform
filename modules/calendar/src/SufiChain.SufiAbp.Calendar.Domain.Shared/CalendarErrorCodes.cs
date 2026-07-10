namespace SufiChain.SufiAbp.Calendar;

public static class CalendarErrorCodes
{
    public const string InvalidTimeRange = "Calendar:InvalidTimeRange";
    public const string OverlappingWorkingHours = "Calendar:OverlappingWorkingHours";
    public const string OverlappingExceptionHours = "Calendar:OverlappingExceptionHours";
    public const string InheritanceCycleDetected = "Calendar:InheritanceCycleDetected";
    public const string CalendarNotAccessible = "Calendar:CalendarNotAccessible";
    public const string UserRequired = "Calendar:UserRequired";
    public const string CalendarCannotInheritItself = "Calendar:CalendarCannotInheritItself";
    public const string InheritanceExceedsOneLevel = "Calendar:InheritanceExceedsOneLevel";
    public const string CalendarInheritanceNotFound = "Calendar:CalendarInheritanceNotFound";
    public const string InvalidTimeZone = "Calendar:InvalidTimeZone";
    public const string DefaultCalendarAlreadyExists = "Calendar:DefaultCalendarAlreadyExists";
    public const string InvalidSource = "Calendar:InvalidSource";
    public const string InvalidRecurrenceRule = "Calendar:InvalidRecurrenceRule";
    public const string InvalidExpansionWindow = "Calendar:InvalidExpansionWindow";
    public const string InvalidOccurrenceOverride = "Calendar:InvalidOccurrenceOverride";
    public const string EventNotRecurring = "Calendar:EventNotRecurring";
    public const string InvalidAttendee = "Calendar:InvalidAttendee";
    public const string OrganizerRequired = "Calendar:OrganizerRequired";
    public const string InvalidReminderOffset = "Calendar:InvalidReminderOffset";
    public const string InvalidFreeBusyWindow = "Calendar:InvalidFreeBusyWindow";
}

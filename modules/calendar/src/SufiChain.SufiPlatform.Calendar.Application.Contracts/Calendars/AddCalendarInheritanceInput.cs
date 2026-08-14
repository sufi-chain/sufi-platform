namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class AddCalendarInheritanceInput
{
    public Guid ParentCalendarId { get; set; }

    /// <summary>
    /// Enables inheritance of the parent calendar's working-hour rules.
    /// Events and exceptions are inherited independently of this value.
    /// </summary>
    public bool IsInheritedByDefault { get; set; }
}

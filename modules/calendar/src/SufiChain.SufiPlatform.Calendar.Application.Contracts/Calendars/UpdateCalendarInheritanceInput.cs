namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class UpdateCalendarInheritanceInput
{
    /// <summary>
    /// Enables inheritance of the parent calendar's working-hour rules.
    /// Events and exceptions are inherited independently of this value.
    /// </summary>
    public bool IsInheritedByDefault { get; set; }
}

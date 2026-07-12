namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class AddCalendarInheritanceInput
{
    public Guid ParentCalendarId { get; set; }

    public bool IsInheritedByDefault { get; set; }
}

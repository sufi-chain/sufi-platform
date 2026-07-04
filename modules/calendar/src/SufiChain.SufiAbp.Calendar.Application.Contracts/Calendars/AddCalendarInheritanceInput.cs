namespace SufiChain.SufiAbp.Calendar.Calendars;

public class AddCalendarInheritanceInput
{
    public Guid ParentCalendarId { get; set; }

    public bool IsInheritedByDefault { get; set; }
}

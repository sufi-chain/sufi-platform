namespace SufiChain.SufiPlatform.Calendar.Events;

public class CreateEventReminderDto
{
    public TimeSpan Offset { get; set; }

    public ReminderChannel Channel { get; set; }

    public Guid? AttendeeId { get; set; }
}

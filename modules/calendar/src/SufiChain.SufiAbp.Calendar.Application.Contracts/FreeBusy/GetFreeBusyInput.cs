namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public class GetFreeBusyInput
{
    public List<Guid> CalendarIds { get; set; } = new();

    public DateTime FromUtc { get; set; }

    public DateTime ToUtc { get; set; }
}

namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public class FindAvailableSlotsInput : GetFreeBusyInput
{
    public TimeSpan Duration { get; set; }
}

namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public class FindAvailableSlotsInput : GetFreeBusyInput
{
    public TimeSpan Duration { get; set; }
}

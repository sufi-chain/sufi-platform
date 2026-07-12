namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public class FreeBusyResultDto
{
    public DateTime FromUtc { get; set; }

    public DateTime ToUtc { get; set; }

    public List<FreeBusySlotDto> BusyBlocks { get; set; } = new();

    public List<FreeBusySlotDto> FreeSlots { get; set; } = new();
}

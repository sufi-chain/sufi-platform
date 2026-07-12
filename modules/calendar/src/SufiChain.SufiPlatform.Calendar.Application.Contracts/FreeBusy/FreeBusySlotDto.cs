namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public class FreeBusySlotDto
{
    public Guid CalendarId { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public int BusyCount { get; set; }
}

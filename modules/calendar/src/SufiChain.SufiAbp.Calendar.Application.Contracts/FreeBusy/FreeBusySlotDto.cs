namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public class FreeBusySlotDto
{
    public Guid CalendarId { get; set; }

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public int BusyCount { get; set; }

    public int? MaxConcurrent { get; set; }

    public bool IsCapacityFull { get; set; }
}

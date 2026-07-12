namespace SufiChain.SufiPlatform.Calendar.Availability;

public class TestAvailabilityResultDto
{
    public bool IsOpen { get; set; }

    public DateTime NextOpenAtUtc { get; set; }

    public DateTime NextCloseAtUtc { get; set; }
}

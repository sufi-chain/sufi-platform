namespace SufiChain.SufiPlatform.Calendar.Events;

public class MoveOccurrenceDto
{
    public DateTime OriginalStartUtc { get; set; }

    public DateTime MovedStartUtc { get; set; }

    public DateTime MovedEndUtc { get; set; }

    public bool ThisAndFollowing { get; set; }
}

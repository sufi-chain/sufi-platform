namespace SufiChain.SufiPlatform.Calendar.Events;

public class CancelOccurrenceDto
{
    public DateTime OriginalStartUtc { get; set; }

    public bool ThisAndFollowing { get; set; }
}

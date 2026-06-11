namespace SufiChain.SufiAbp.Calendar.Calendars;

public class WorkingHourRangeDto
{
    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public int? MaxConcurrent { get; set; }
}

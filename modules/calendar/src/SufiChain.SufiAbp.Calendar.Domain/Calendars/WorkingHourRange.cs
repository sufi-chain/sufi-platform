namespace SufiChain.SufiAbp.Calendar.Calendars;

public sealed record WorkingHourRange(TimeOnly StartTime, TimeOnly EndTime, int? MaxConcurrent = null);

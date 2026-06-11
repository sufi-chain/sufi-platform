namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public sealed record BusyBlock(Guid CalendarId, DateTime StartUtc, DateTime EndUtc, int BusyCount, int? MaxConcurrent)
{
    public bool IsCapacityFull => MaxConcurrent.HasValue && BusyCount >= MaxConcurrent.Value;
}

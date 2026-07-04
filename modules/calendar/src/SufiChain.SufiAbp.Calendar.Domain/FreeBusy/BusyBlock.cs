namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public sealed record BusyBlock(Guid CalendarId, DateTime StartUtc, DateTime EndUtc, int BusyCount);

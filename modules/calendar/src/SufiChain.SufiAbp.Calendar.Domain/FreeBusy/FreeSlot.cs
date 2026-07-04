namespace SufiChain.SufiAbp.Calendar.FreeBusy;

public sealed record FreeSlot(Guid CalendarId, DateTime StartUtc, DateTime EndUtc);

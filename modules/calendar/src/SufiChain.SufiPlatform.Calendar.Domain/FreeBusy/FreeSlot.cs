namespace SufiChain.SufiPlatform.Calendar.FreeBusy;

public sealed record FreeSlot(Guid CalendarId, DateTime StartUtc, DateTime EndUtc);

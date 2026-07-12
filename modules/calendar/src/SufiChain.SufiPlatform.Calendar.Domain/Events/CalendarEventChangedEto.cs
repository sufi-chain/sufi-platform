namespace SufiChain.SufiPlatform.Calendar.Events;

public sealed record CalendarEventChangedEto(Guid EventId, Guid CalendarId, Guid? TenantId);

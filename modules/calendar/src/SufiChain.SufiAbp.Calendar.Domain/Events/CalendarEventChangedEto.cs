namespace SufiChain.SufiAbp.Calendar.Events;

public sealed record CalendarEventChangedEto(Guid EventId, Guid CalendarId, Guid? TenantId);

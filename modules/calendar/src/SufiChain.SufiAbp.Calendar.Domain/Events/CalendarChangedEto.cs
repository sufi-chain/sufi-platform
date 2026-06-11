namespace SufiChain.SufiAbp.Calendar.Events;

public sealed record CalendarChangedEto(Guid CalendarId, Guid? TenantId);

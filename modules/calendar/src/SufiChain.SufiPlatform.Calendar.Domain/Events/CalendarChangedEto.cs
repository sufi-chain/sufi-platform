namespace SufiChain.SufiPlatform.Calendar.Events;

public sealed record CalendarChangedEto(Guid CalendarId, Guid? TenantId);

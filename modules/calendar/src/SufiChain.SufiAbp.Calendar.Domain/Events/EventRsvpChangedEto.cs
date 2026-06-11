namespace SufiChain.SufiAbp.Calendar.Events;

public sealed record EventRsvpChangedEto(Guid EventId, Guid CalendarId, Guid AttendeeId, RsvpStatus RsvpStatus, Guid? TenantId);

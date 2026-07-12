namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public sealed record SufiCalendarSlotSelectArgs(DateTime StartUtc, DateTime EndUtc, IReadOnlyList<Guid> CalendarIds);

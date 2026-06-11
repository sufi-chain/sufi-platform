using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.Reminders;

public sealed record EventReminderDispatchItem(
    CalendarEvent Event,
    EventReminder Reminder,
    EventOccurrence Occurrence,
    EventAttendee? Attendee,
    DateTime DueAtUtc);

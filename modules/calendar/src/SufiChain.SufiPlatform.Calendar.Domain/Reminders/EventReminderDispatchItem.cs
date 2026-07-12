using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

public sealed record EventReminderDispatchItem(
    CalendarEvent Event,
    EventReminder Reminder,
    EventOccurrence Occurrence,
    EventAttendee? Attendee,
    DateTime DueAtUtc);

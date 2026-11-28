using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

/// <summary>
/// Records a skipped reminder on the event's existing extra-properties bag.
/// A new column would need a migration, which this fix is not allowed to add.
/// <see cref="EventReminder.MarkSkipped"/> also sets <c>SentAtUtc</c> so the
/// unsent query stops reloading the event.
/// </summary>
public static class CalendarReminderSkip
{
    public static string Key(Guid reminderId) => "ReminderSkipped:" + reminderId.ToString("N");

    public static bool IsRecorded(CalendarEvent calendarEvent, Guid reminderId)
    {
        return calendarEvent.ExtraProperties.ContainsKey(Key(reminderId));
    }
}

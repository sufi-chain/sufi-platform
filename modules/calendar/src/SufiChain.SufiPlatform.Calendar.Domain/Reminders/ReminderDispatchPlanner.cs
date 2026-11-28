using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Scheduling;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

/// <summary>
/// Picks reminders whose due time has arrived. A due time inside the catch-up
/// window is delivered when the occurrence has not ended. An older due time, or
/// an occurrence that has already ended, is returned with <see cref="EventReminderDispatchItem.Skip"/>
/// so the caller can mark it and stop reloading it.
/// </summary>
public static class ReminderDispatchPlanner
{
    public static List<EventReminderDispatchItem> Select(
        IEnumerable<CalendarEvent> events,
        DateTime nowUtc,
        TimeSpan catchUp)
    {
        var now = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
        if (catchUp < TimeSpan.Zero)
        {
            catchUp = TimeSpan.Zero;
        }

        var calculator = new RecurrenceCalculator();
        var items = new List<EventReminderDispatchItem>();
        foreach (var calendarEvent in events)
        {
            foreach (var reminder in calendarEvent.Reminders)
            {
                if (reminder.SentAtUtc != null || CalendarReminderSkip.IsRecorded(calendarEvent, reminder.Id))
                {
                    continue;
                }

                if (!TrySelect(calculator, calendarEvent, reminder, now, catchUp, out var occurrence, out var dueAt, out var skip))
                {
                    continue;
                }

                items.Add(new EventReminderDispatchItem(
                    calendarEvent,
                    reminder,
                    occurrence,
                    ResolveAttendee(calendarEvent, reminder),
                    dueAt,
                    skip));
            }
        }

        return items;
    }

    private static bool TrySelect(
        RecurrenceCalculator calculator,
        CalendarEvent calendarEvent,
        EventReminder reminder,
        DateTime now,
        TimeSpan catchUp,
        out EventOccurrence occurrence,
        out DateTime dueAt,
        out bool skip)
    {
        occurrence = default!;
        dueAt = default;
        skip = false;

        if (calendarEvent.RecurrenceRule is null)
        {
            dueAt = DateTime.SpecifyKind(calendarEvent.StartUtc.Add(reminder.Offset), DateTimeKind.Utc);
            if (dueAt > now)
            {
                return false;
            }

            var windowStart = calendarEvent.StartUtc.AddMinutes(-1);
            var windowEnd = calendarEvent.EndUtc > windowStart
                ? calendarEvent.EndUtc.AddMinutes(1)
                : windowStart.AddMinutes(1);
            var expanded = calculator.Expand(calendarEvent, windowStart, windowEnd);
            if (expanded.Count == 0)
            {
                return false;
            }

            occurrence = expanded[0];
            skip = dueAt <= now.Subtract(catchUp) || occurrence.EndUtc <= now;
            return true;
        }

        var offset = reminder.Offset;
        var windowEndUtc = now.Subtract(offset).AddMinutes(1);
        var windowStartUtc = now.Subtract(catchUp).Subtract(TimeSpan.FromDays(EventConsts.MaxReminderOffsetDays));
        var maxWindow = TimeSpan.FromDays(EventConsts.MaxExpansionWindowDays - 1);
        if (windowEndUtc - windowStartUtc > maxWindow)
        {
            windowStartUtc = windowEndUtc - maxWindow;
        }

        if (windowEndUtc <= windowStartUtc)
        {
            windowEndUtc = windowStartUtc.AddMinutes(1);
        }

        EventOccurrence? dispatchOccurrence = null;
        var dispatchDue = DateTime.MinValue;
        EventOccurrence? skipOccurrence = null;
        var skipDue = DateTime.MinValue;
        foreach (var candidate in calculator.Expand(calendarEvent, windowStartUtc, windowEndUtc))
        {
            var candidateDue = DateTime.SpecifyKind(candidate.StartUtc.Add(offset), DateTimeKind.Utc);
            if (candidateDue > now)
            {
                continue;
            }

            var inCatchUp = candidateDue > now.Subtract(catchUp) && candidate.EndUtc > now;
            if (inCatchUp)
            {
                if (dispatchOccurrence is null || candidateDue >= dispatchDue)
                {
                    dispatchOccurrence = candidate;
                    dispatchDue = candidateDue;
                }
            }
            else if (skipOccurrence is null || candidateDue >= skipDue)
            {
                skipOccurrence = candidate;
                skipDue = candidateDue;
            }
        }

        if (dispatchOccurrence is not null)
        {
            occurrence = dispatchOccurrence;
            dueAt = dispatchDue;
            skip = false;
            return true;
        }

        if (skipOccurrence is not null)
        {
            occurrence = skipOccurrence;
            dueAt = skipDue;
            skip = true;
            return true;
        }

        return false;
    }

    private static EventAttendee? ResolveAttendee(CalendarEvent calendarEvent, EventReminder reminder)
    {
        if (reminder.AttendeeId.HasValue)
        {
            return calendarEvent.Attendees.FirstOrDefault(x => x.Id == reminder.AttendeeId.Value);
        }

        return calendarEvent.Attendees.FirstOrDefault(x => x.Role == AttendeeRole.Organizer)
            ?? calendarEvent.Attendees.FirstOrDefault();
    }
}

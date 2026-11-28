using Shouldly;
using SufiChain.SufiPlatform.Calendar.Events;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore.Reminders;

/// <summary>
/// CAL-13: a reminder due a few minutes ago must still be found. The old ±1 minute
/// window dropped it, so a late worker run never sent it.
/// </summary>
public class CalendarReminderWindowTests : CalendarEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task GetDueReminders_DueThreeMinutesAgo_WithinCatchUp_Fires()
    {
        // A 90-second event that started a minute ago. Its reminder (2 minutes before)
        // was due three minutes ago and the event has not ended. The old ±1 minute
        // window sits ahead of the event and drops it.
        var now = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);
        var start = now.AddMinutes(-1);
        var calendarEvent = Event(start, start.AddSeconds(90));
        calendarEvent.AddReminder(new EventReminder(
            Guid.NewGuid(),
            calendarEvent.Id,
            TimeSpan.FromMinutes(-2),
            ReminderChannel.InApp));

        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarEventRepository>().InsertAsync(calendarEvent, autoSave: true));

        await WithUnitOfWorkAsync(async () =>
        {
            var due = await GetRequiredService<ICalendarEventRepository>().GetDueRemindersAsync(now);
            due.Count.ShouldBe(1);
            due[0].Skip.ShouldBeFalse();
            due[0].DueAtUtc.ShouldBe(start.AddMinutes(-2));
        });
    }

    [Fact]
    public async Task GetDueReminders_DuePastCatchUp_IsStillReturnedSoItCanBeSkipped()
    {
        var now = new DateTime(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);
        var start = now.AddMinutes(-30);
        var calendarEvent = Event(start, start.AddMinutes(10));
        calendarEvent.AddReminder(new EventReminder(
            Guid.NewGuid(),
            calendarEvent.Id,
            TimeSpan.FromMinutes(-10),
            ReminderChannel.Email));

        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarEventRepository>().InsertAsync(calendarEvent, autoSave: true));

        await WithUnitOfWorkAsync(async () =>
        {
            var repository = GetRequiredService<ICalendarEventRepository>();
            var due = await repository.GetDueRemindersAsync(now);
            due.Count.ShouldBe(1);
            due[0].Skip.ShouldBeTrue();
            due[0].Event.MarkReminderSkipped(due[0].Reminder, now, "past-catch-up");
            await repository.UpdateAsync(due[0].Event, autoSave: true);

            var again = await repository.GetDueRemindersAsync(now);
            again.ShouldBeEmpty();
        });
    }

    private static CalendarEvent Event(DateTime startUtc, DateTime endUtc)
    {
        return new CalendarEvent(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            "Planning",
            startUtc,
            endUtc,
            isAllDay: false,
            "Asia/Tehran");
    }
}

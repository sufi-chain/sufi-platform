using Shouldly;
using SufiChain.SufiPlatform.Calendar.Events;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.MongoDB.Reminders;

public class CalendarReminderWindowMongoTests : CalendarMongoDBTestBase
{
    [Fact]
    public async Task GetDueReminders_DueThreeMinutesAgo_WithinCatchUp_Fires()
    {
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
    public async Task GetDueReminders_DuePastCatchUp_MarkedSkipped_NotReloaded()
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
            (await repository.GetDueRemindersAsync(now)).ShouldBeEmpty();
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
            "UTC");
    }
}

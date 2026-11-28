using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore.Reminders;

public class CalendarEventCreateWithReminderTests : CalendarEntityFrameworkCoreTestBase
{
    [Fact]
    public async Task EventEditor_NewEvent_CanAddReminderBeforeSave()
    {
        var calendarId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(calendarId, null, "Team", CalendarKind.Public, "UTC"), autoSave: true));

        var created = await GetRequiredService<ICalendarEventAppService>().CreateAsync(new CreateUpdateCalendarEventDto
        {
            CalendarId = calendarId,
            Title = "Planning",
            StartUtc = new DateTime(2026, 11, 28, 9, 0, 0, DateTimeKind.Utc),
            EndUtc = new DateTime(2026, 11, 28, 10, 0, 0, DateTimeKind.Utc),
            TimeZoneId = "UTC",
            Attendees =
            {
                new CreateEventAttendeeDto { DisplayName = "Ada", Email = "ada@example.com", Role = AttendeeRole.Required }
            },
            Reminders =
            {
                new CreateEventReminderDto { Offset = TimeSpan.FromMinutes(-15), Channel = ReminderChannel.Email }
            }
        });

        created.Attendees.Count.ShouldBe(1);
        created.Reminders.Count.ShouldBe(1);
        created.Reminders[0].Offset.ShouldBe(TimeSpan.FromMinutes(-15));
        created.Reminders[0].Channel.ShouldBe(ReminderChannel.Email);

        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id);
            saved.Reminders.Single().Channel.ShouldBe(ReminderChannel.Email);
            saved.Attendees.Single().DisplayName.ShouldBe("Ada");
        });
    }

    [Fact]
    public async Task Create_Rejects_A_Reminder_Offset_Past_Thirty_Days()
    {
        var calendarId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(calendarId, null, "Team", CalendarKind.Public, "UTC"), autoSave: true));

        var error = await Should.ThrowAsync<AbpValidationException>(() =>
            GetRequiredService<ICalendarEventAppService>().CreateAsync(new CreateUpdateCalendarEventDto
            {
                CalendarId = calendarId,
                Title = "Planning",
                StartUtc = new DateTime(2026, 11, 28, 9, 0, 0, DateTimeKind.Utc),
                EndUtc = new DateTime(2026, 11, 28, 10, 0, 0, DateTimeKind.Utc),
                TimeZoneId = "UTC",
                Reminders =
                {
                    new CreateEventReminderDto { Offset = TimeSpan.FromDays(-31), Channel = ReminderChannel.InApp }
                }
            }));

        error.ValidationErrors.ShouldContain(item => item.MemberNames.Contains(nameof(CreateEventReminderDto.Offset)));
    }
}

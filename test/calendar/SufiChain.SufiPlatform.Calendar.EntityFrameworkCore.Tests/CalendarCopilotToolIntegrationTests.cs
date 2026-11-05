using Shouldly;
using SufiChain.SufiPlatform.Calendar.AI.Tools;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.FreeBusy;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

public class CalendarCopilotToolIntegrationTests : CalendarEntityFrameworkCoreTestBase
{
    private static readonly DateTime WindowStart = new(2026, 9, 8, 8, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CreatedStart = new(2026, 9, 8, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime CreatedEnd = new(2026, 9, 8, 11, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime MovedStart = new(2026, 9, 8, 14, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime MovedEnd = new(2026, 9, 8, 15, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime WindowEnd = new(2026, 9, 8, 18, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Create_Move_And_Find_Free_Slot_Tools_Use_The_Calendar()
    {
        var calendarId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(
                    calendarId,
                    CurrentTenant.Id,
                    "Open calendar",
                    CalendarKind.Public,
                    "UTC",
                    isAlwaysOpen: true),
                autoSave: true));

        var events = GetRequiredService<ICalendarEventAppService>();
        var create = new CalendarCreateEventTool(events);
        var move = new CalendarMoveEventTool(events);
        var find = new CalendarFindFreeSlotsTool(GetRequiredService<IFreeBusyAppService>());

        var created = await create.CreateEventAsync(
            calendarId,
            "Planning",
            CreatedStart,
            CreatedEnd,
            timeZoneId: "UTC");
        created.Id.ShouldNotBe(Guid.Empty);
        created.StartUtc.ShouldBe(CreatedStart);
        created.EndUtc.ShouldBe(CreatedEnd);

        var moved = await move.MoveEventAsync(created.Id, MovedStart, MovedEnd);
        moved.Title.ShouldBe("Planning");
        moved.StartUtc.ShouldBe(MovedStart);
        moved.EndUtc.ShouldBe(MovedEnd);

        var raw = await find.FindFreeSlotsAsync(
            new List<Guid> { calendarId },
            WindowStart,
            WindowEnd,
            TimeSpan.FromMinutes(30));
        var slots = raw.ShouldBeAssignableTo<IReadOnlyList<FreeBusySlotDto>>();
        slots.ShouldNotBeEmpty();
        slots.ShouldAllBe(slot => slot.EndUtc <= MovedStart || slot.StartUtc >= MovedEnd);
        slots.ShouldContain(slot => slot.StartUtc <= CreatedStart && slot.EndUtc >= CreatedEnd);
    }
}

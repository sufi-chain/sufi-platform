using Microsoft.EntityFrameworkCore;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.EntityFrameworkCore;

public class CalendarRecurrenceIntegrationTests : CalendarEntityFrameworkCoreTestBase
{
    private static readonly DateTime Start = new(2026, 9, 8, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task Should_Persist_Moved_And_Cancelled_Occurrences_And_Refresh_Expansion()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var created = await service.CreateAsync(NewEvent(calendarId));
        (await service.GetOccurrencesAsync(calendarId, Window())).Items.Count.ShouldBe(3);

        await service.MoveOccurrenceAsync(created.Id, new MoveOccurrenceDto
        {
            OriginalStartUtc = Start.AddDays(1),
            MovedStartUtc = Start.AddDays(1).AddHours(2),
            MovedEndUtc = Start.AddDays(1).AddHours(3)
        });
        await service.CancelOccurrenceAsync(created.Id, new CancelOccurrenceDto
        {
            OriginalStartUtc = Start.AddDays(2)
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id, includeDetails: true);
            saved.RecurrenceRule.ShouldNotBeNull();
            saved.RecurrenceRule.Rule.ShouldBe("FREQ=DAILY;COUNT=3");
            saved.OccurrenceExceptions.Count.ShouldBe(2);
            var moved = saved.OccurrenceExceptions.Single(x => x.OriginalStartUtc == Start.AddDays(1));
            moved.IsCancelled.ShouldBeFalse();
            moved.OverrideStartUtc.ShouldBe(Start.AddDays(1).AddHours(2));
            moved.OverrideEndUtc.ShouldBe(Start.AddDays(1).AddHours(3));
            saved.OccurrenceExceptions.Single(x => x.OriginalStartUtc == Start.AddDays(2)).IsCancelled.ShouldBeTrue();
        });

        var expanded = (await service.GetOccurrencesAsync(calendarId, Window())).Items;
        expanded.Count.ShouldBe(2);
        expanded.Select(x => x.StartUtc).ShouldBe(new[] { Start, Start.AddDays(1).AddHours(2) });
        expanded.Single(x => x.OriginalStartUtc == Start.AddDays(1)).EndUtc.ShouldBe(Start.AddDays(1).AddHours(3));
    }

    [Fact]
    public async Task Should_Replace_Persisted_Override_Without_Duplicating_The_Occurrence()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var created = await service.CreateAsync(NewEvent(calendarId));
        await service.CancelOccurrenceAsync(created.Id, new CancelOccurrenceDto { OriginalStartUtc = Start });

        await service.MoveOccurrenceAsync(created.Id, new MoveOccurrenceDto
        {
            OriginalStartUtc = Start,
            MovedStartUtc = Start.AddHours(2),
            MovedEndUtc = Start.AddHours(3)
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id, includeDetails: true);
            saved.OccurrenceExceptions.ShouldHaveSingleItem().IsCancelled.ShouldBeFalse();
            var db = await GetRequiredService<IDbContextProvider<CalendarDbContext>>().GetDbContextAsync();
            (await db.EventOccurrenceExceptions.CountAsync(x => x.EventId == created.Id)).ShouldBe(1);
        });
        var occurrences = (await service.GetOccurrencesAsync(calendarId, Window())).Items;
        occurrences.Count.ShouldBe(3);
        occurrences.Single(x => x.OriginalStartUtc == Start).StartUtc.ShouldBe(Start.AddHours(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Should_Keep_Persisted_Cancellation_When_Move_Range_Is_Rejected(int durationHours)
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var created = await service.CreateAsync(NewEvent(calendarId));
        await service.CancelOccurrenceAsync(created.Id, new CancelOccurrenceDto { OriginalStartUtc = Start });

        var error = await Should.ThrowAsync<BusinessException>(() => service.MoveOccurrenceAsync(created.Id,
            new MoveOccurrenceDto
            {
                OriginalStartUtc = Start,
                MovedStartUtc = Start.AddHours(2),
                MovedEndUtc = Start.AddHours(2 + durationHours)
            }));

        error.Code.ShouldBe(CalendarErrorCodes.InvalidOccurrenceOverride);
        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id, includeDetails: true);
            var cancellation = saved.OccurrenceExceptions.ShouldHaveSingleItem();
            cancellation.OriginalStartUtc.ShouldBe(Start);
            cancellation.IsCancelled.ShouldBeTrue();
            cancellation.OverrideStartUtc.ShouldBeNull();
            cancellation.OverrideEndUtc.ShouldBeNull();
        });
    }

    [Fact]
    public async Task Should_Delete_Stored_Exceptions_And_Rule_When_Recurrence_Is_Cleared()
    {
        var calendarId = await CreateCalendarAsync();
        var service = GetRequiredService<ICalendarEventAppService>();
        var created = await service.CreateAsync(NewEvent(calendarId));
        await service.CancelOccurrenceAsync(created.Id, new CancelOccurrenceDto { OriginalStartUtc = Start.AddDays(1) });
        (await service.GetOccurrencesAsync(calendarId, Window())).Items.Count.ShouldBe(2);

        await service.ClearRecurrenceAsync(created.Id);

        await WithUnitOfWorkAsync(async () =>
        {
            var saved = await GetRequiredService<ICalendarEventRepository>().GetAsync(created.Id, includeDetails: true);
            saved.RecurrenceRule.ShouldBeNull();
            saved.OccurrenceExceptions.ShouldBeEmpty();
            var db = await GetRequiredService<IDbContextProvider<CalendarDbContext>>().GetDbContextAsync();
            (await db.EventOccurrenceExceptions.CountAsync(x => x.EventId == created.Id)).ShouldBe(0);
            (await db.Set<RecurrenceRule>().CountAsync(x => x.EventId == created.Id)).ShouldBe(0);
        });
        (await service.GetOccurrencesAsync(calendarId, Window())).Items.ShouldHaveSingleItem().StartUtc.ShouldBe(Start);
    }

    private async Task<Guid> CreateCalendarAsync()
    {
        var id = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
            await GetRequiredService<ICalendarRepository>().InsertAsync(
                new Calendars.Calendar(id, null, "Recurrence tests", CalendarKind.Public, "UTC"), autoSave: true));
        return id;
    }

    private static CreateUpdateCalendarEventDto NewEvent(Guid calendarId) => new()
    {
        CalendarId = calendarId,
        Title = "Daily planning",
        StartUtc = Start,
        EndUtc = Start.AddHours(1),
        TimeZoneId = "UTC",
        RecurrenceRule = "FREQ=DAILY;COUNT=3"
    };

    private static GetOccurrencesInput Window() => new() { FromUtc = Start.Date, ToUtc = Start.Date.AddDays(4) };
}

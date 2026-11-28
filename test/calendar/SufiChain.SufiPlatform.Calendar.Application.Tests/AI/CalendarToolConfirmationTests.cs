using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.AI;
using SufiChain.SufiPlatform.Calendar.AI.Tools;
using SufiChain.SufiPlatform.Calendar.Events;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar;

public class CalendarToolConfirmationTests
{
    private static readonly Guid EventId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
    private static readonly Guid CalendarId = Guid.Parse("11111111-2222-3333-4444-555555555555");
    private static readonly DateTime Start = new(2026, 11, 28, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 11, 28, 10, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime MovedStart = new(2026, 11, 28, 11, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime MovedEnd = new(2026, 11, 28, 12, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task MoveEvent_RequiresConfirmationBeforeUpdate()
    {
        var existing = Event();
        var events = Substitute.For<ICalendarEventAppService>();
        events.GetAsync(EventId).Returns(existing);

        var preview = await new CalendarMoveEventTool(events).MoveEventAsync(EventId, MovedStart, MovedEnd);

        preview.RequiresConfirmation.ShouldBeTrue();
        preview.Id.ShouldBe(EventId);
        preview.StartUtc.ShouldBe(MovedStart);
        preview.EndUtc.ShouldBe(MovedEnd);
        preview.ConfirmationSummary.ShouldBe("move");
        await events.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<CreateUpdateCalendarEventDto>());
    }

    [Fact]
    public async Task MoveEvent_Decline_DoesNotChangeEvent()
    {
        var existing = Event();
        var events = Substitute.For<ICalendarEventAppService>();
        events.GetAsync(EventId).Returns(existing);

        var preview = await new CalendarMoveEventTool(events).MoveEventAsync(EventId, MovedStart, MovedEnd, confirmed: false);

        preview.RequiresConfirmation.ShouldBeTrue();
        existing.StartUtc.ShouldBe(Start);
        existing.EndUtc.ShouldBe(End);
        await events.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<CreateUpdateCalendarEventDto>());
    }

    [Fact]
    public async Task MoveEvent_Confirmed_UpdatesEvent()
    {
        var existing = Event();
        var events = Substitute.For<ICalendarEventAppService>();
        events.GetAsync(EventId).Returns(existing);
        events.UpdateAsync(EventId, Arg.Any<CreateUpdateCalendarEventDto>())
            .Returns(call =>
            {
                var input = call.Arg<CreateUpdateCalendarEventDto>();
                return Event(input.StartUtc, input.EndUtc);
            });

        var moved = await new CalendarMoveEventTool(events).MoveEventAsync(EventId, MovedStart, MovedEnd, confirmed: true);

        moved.RequiresConfirmation.ShouldBeFalse();
        moved.StartUtc.ShouldBe(MovedStart);
        moved.EndUtc.ShouldBe(MovedEnd);
    }

    [Fact]
    public async Task CancelEvent_And_OccurrenceMutations_RequireConfirmation()
    {
        var existing = Event();
        var events = Substitute.For<ICalendarEventAppService>();
        events.GetAsync(EventId).Returns(existing);

        var cancel = await new CalendarCancelEventTool(events).CancelEventAsync(EventId);
        var moveOccurrence = await new CalendarMoveOccurrenceTool(events).MoveOccurrenceAsync(EventId, Start, MovedStart, MovedEnd);
        var cancelOccurrence = await new CalendarCancelOccurrenceTool(events).CancelOccurrenceAsync(EventId, Start);
        var create = await new CalendarCreateEventTool(events).CreateEventAsync(CalendarId, "Planning", Start, End);

        cancel.RequiresConfirmation.ShouldBeTrue();
        cancel.ConfirmationSummary.ShouldBe("cancel");
        moveOccurrence.RequiresConfirmation.ShouldBeTrue();
        moveOccurrence.ConfirmationSummary.ShouldBe("move-occurrence");
        cancelOccurrence.RequiresConfirmation.ShouldBeTrue();
        cancelOccurrence.ConfirmationSummary.ShouldBe("cancel-occurrence");
        create.RequiresConfirmation.ShouldBeTrue();
        create.ConfirmationSummary.ShouldBe("create");
        create.Id.ShouldBe(Guid.Empty);

        await events.DidNotReceive().UpdateAsync(Arg.Any<Guid>(), Arg.Any<CreateUpdateCalendarEventDto>());
        await events.DidNotReceive().CreateAsync(Arg.Any<CreateUpdateCalendarEventDto>());
        await events.DidNotReceive().MoveOccurrenceAsync(Arg.Any<Guid>(), Arg.Any<MoveOccurrenceDto>());
        await events.DidNotReceive().CancelOccurrenceAsync(Arg.Any<Guid>(), Arg.Any<CancelOccurrenceDto>());
        existing.StartUtc.ShouldBe(Start);
    }

    [Fact]
    public void CalendarAssistant_ThinkingClearsWhenStreamEnds()
    {
        CalendarAssistantThinking.ClearsWhenStreamEnds("writing_reply", "succeeded").ShouldBeTrue();
        CalendarAssistantThinking.ClearsWhenConfirmationShows("tool_result", "succeeded").ShouldBeTrue();
        CalendarAssistantThinking.ClearsWhenStreamEnds("writing_reply", "started").ShouldBeFalse();
        CalendarAssistantThinking.ClearsWhenStreamEnds("preparing", "succeeded").ShouldBeFalse();
    }

    [Fact]
    public void CalendarAssistant_LateReplyAfter45sHint_IsShown()
    {
        CalendarAssistantThinking.ShowsLateReplyAfterHint().ShouldBeTrue();
    }

    private static CalendarEventDto Event(DateTime? start = null, DateTime? end = null) => new()
    {
        Id = EventId,
        CalendarId = CalendarId,
        Title = "Planning",
        StartUtc = start ?? Start,
        EndUtc = end ?? End,
        TimeZoneId = "UTC",
        Status = EventStatus.Confirmed
    };
}

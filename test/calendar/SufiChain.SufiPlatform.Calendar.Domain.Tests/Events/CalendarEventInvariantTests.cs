using System;
using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CalendarEventInvariantTests
{
    private static readonly DateTime Start = new(2026, 9, 8, 9, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Preserve_Time_Range_When_End_Is_Not_After_Start(int endOffsetHours)
    {
        var calendarEvent = CreateEvent();

        var exception = Should.Throw<BusinessException>(() =>
            calendarEvent.SetTimeRange(Start, Start.AddHours(endOffsetHours), true, "UTC"));

        exception.Code.ShouldBe(CalendarErrorCodes.InvalidTimeRange);
        calendarEvent.StartUtc.ShouldBe(Start);
        calendarEvent.EndUtc.ShouldBe(Start.AddHours(1));
        calendarEvent.IsAllDay.ShouldBeFalse();
    }

    [Theory]
    [InlineData("ticket", null)]
    [InlineData(null, "123")]
    [InlineData("ticket", " ")]
    public void Should_Preserve_Source_When_Only_One_Source_Field_Is_Supplied(string? sourceType, string? sourceId)
    {
        var calendarEvent = CreateEvent();
        calendarEvent.AttachSource("ticket", "original");

        var exception = Should.Throw<BusinessException>(() => calendarEvent.AttachSource(sourceType, sourceId));

        exception.Code.ShouldBe(CalendarErrorCodes.InvalidSource);
        calendarEvent.SourceType.ShouldBe("ticket");
        calendarEvent.SourceId.ShouldBe("original");
    }

    [Fact]
    public void Should_Detach_Source_When_Both_Fields_Are_Cleared()
    {
        var calendarEvent = CreateEvent();
        calendarEvent.AttachSource("ticket", "123");

        calendarEvent.AttachSource(null, null);

        calendarEvent.SourceType.ShouldBeNull();
        calendarEvent.SourceId.ShouldBeNull();
    }

    [Fact]
    public void Should_Reject_Attendee_From_Another_Event()
    {
        var calendarEvent = CreateEvent();
        var attendee = CreateOrganizer(Guid.NewGuid());

        var exception = Should.Throw<BusinessException>(() => calendarEvent.AddAttendee(attendee));

        exception.Code.ShouldBe(CalendarErrorCodes.InvalidAttendee);
        calendarEvent.Attendees.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Keep_Original_Organizer_When_Second_Organizer_Is_Rejected()
    {
        var calendarEvent = CreateEvent();
        var organizer = CreateOrganizer(calendarEvent.Id);
        calendarEvent.AddAttendee(organizer);

        var exception = Should.Throw<BusinessException>(() =>
            calendarEvent.AddAttendee(CreateOrganizer(calendarEvent.Id)));

        exception.Code.ShouldBe(CalendarErrorCodes.OrganizerRequired);
        calendarEvent.Attendees.Single().ShouldBeSameAs(organizer);
    }

    [Fact]
    public void Should_Reject_Removal_Of_Only_Organizer()
    {
        var calendarEvent = CreateEvent();
        var organizer = CreateOrganizer(calendarEvent.Id);
        calendarEvent.AddAttendee(organizer);

        var exception = Should.Throw<BusinessException>(() => calendarEvent.RemoveAttendee(organizer.Id));

        exception.Code.ShouldBe(CalendarErrorCodes.OrganizerRequired);
        calendarEvent.Attendees.Single().ShouldBeSameAs(organizer);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Should_Reject_Reminder_With_Foreign_Event_Or_Unknown_Attendee(bool foreignEvent)
    {
        var calendarEvent = CreateEvent();
        var reminder = new EventReminder(
            Guid.NewGuid(), foreignEvent ? Guid.NewGuid() : calendarEvent.Id,
            TimeSpan.FromMinutes(-15), ReminderChannel.Email,
            foreignEvent ? null : Guid.NewGuid());

        var exception = Should.Throw<BusinessException>(() => calendarEvent.AddReminder(reminder));

        exception.Code.ShouldBe(CalendarErrorCodes.InvalidAttendee);
        calendarEvent.Reminders.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Accept_Reminder_For_Existing_Attendee()
    {
        var calendarEvent = CreateEvent();
        var organizer = CreateOrganizer(calendarEvent.Id);
        calendarEvent.AddAttendee(organizer);
        var reminder = new EventReminder(Guid.NewGuid(), calendarEvent.Id,
            TimeSpan.FromMinutes(-15), ReminderChannel.Email, organizer.Id);

        calendarEvent.AddReminder(reminder);

        calendarEvent.Reminders.Single().AttendeeId.ShouldBe(organizer.Id);
        calendarEvent.Reminders.Single().Offset.ShouldBe(TimeSpan.FromMinutes(-15));
    }

    [Fact]
    public void Should_Reject_Override_When_Event_Is_Not_Recurring()
    {
        var calendarEvent = CreateEvent();

        var exception = Should.Throw<BusinessException>(() =>
            calendarEvent.CancelOccurrence(Guid.NewGuid(), Start));

        exception.Code.ShouldBe(CalendarErrorCodes.EventNotRecurring);
        calendarEvent.OccurrenceExceptions.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Reject_Override_From_Another_Event()
    {
        var calendarEvent = CreateEvent();
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=3");
        var foreignOverride = EventOccurrenceException.Cancel(Guid.NewGuid(), Guid.NewGuid(), Start);

        var exception = Should.Throw<BusinessException>(() =>
            calendarEvent.AddOrReplaceOccurrenceException(foreignOverride));

        exception.Code.ShouldBe(CalendarErrorCodes.InvalidOccurrenceOverride);
        calendarEvent.OccurrenceExceptions.ShouldBeEmpty();
    }

    [Fact]
    public void Should_Replace_Only_The_Matching_Occurrence_Override()
    {
        var calendarEvent = CreateEvent();
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=3");
        calendarEvent.CancelOccurrence(Guid.NewGuid(), Start);
        calendarEvent.CancelOccurrence(Guid.NewGuid(), Start.AddDays(1));

        calendarEvent.MoveOccurrence(Guid.NewGuid(), Start, Start.AddHours(2), Start.AddHours(3));

        calendarEvent.OccurrenceExceptions.Count.ShouldBe(2);
        var moved = calendarEvent.OccurrenceExceptions.Single(x => x.OriginalStartUtc == Start);
        moved.IsCancelled.ShouldBeFalse();
        moved.OverrideStartUtc.ShouldBe(Start.AddHours(2));
        moved.OverrideEndUtc.ShouldBe(Start.AddHours(3));
        calendarEvent.OccurrenceExceptions.Single(x => x.OriginalStartUtc == Start.AddDays(1))
            .IsCancelled.ShouldBeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Should_Preserve_Existing_Override_When_Move_Range_Is_Invalid(int endOffsetHours)
    {
        var calendarEvent = CreateEvent();
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=3");
        var cancellationId = Guid.NewGuid();
        calendarEvent.CancelOccurrence(cancellationId, Start);

        var exception = Should.Throw<BusinessException>(() => calendarEvent.MoveOccurrence(
            Guid.NewGuid(), Start, Start, Start.AddHours(endOffsetHours)));

        exception.Code.ShouldBe(CalendarErrorCodes.InvalidOccurrenceOverride);
        calendarEvent.OccurrenceExceptions.Single().Id.ShouldBe(cancellationId);
        calendarEvent.OccurrenceExceptions.Single().IsCancelled.ShouldBeTrue();
    }

    [Fact]
    public void Should_Clear_Overrides_When_Recurrence_Is_Removed()
    {
        var calendarEvent = CreateEvent();
        calendarEvent.SetRecurrence(Guid.NewGuid(), "FREQ=DAILY;COUNT=3");
        calendarEvent.CancelOccurrence(Guid.NewGuid(), Start.AddDays(1));

        calendarEvent.ClearRecurrence();

        calendarEvent.RecurrenceRule.ShouldBeNull();
        calendarEvent.OccurrenceExceptions.ShouldBeEmpty();
        calendarEvent.StartUtc.ShouldBe(Start);
        calendarEvent.EndUtc.ShouldBe(Start.AddHours(1));
    }

    private static CalendarEvent CreateEvent() => new(
        Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Planning session",
        Start, Start.AddHours(1), false, "UTC");

    private static EventAttendee CreateOrganizer(Guid eventId) => new(
        Guid.NewGuid(), eventId, Guid.NewGuid(), null, "Organizer", AttendeeRole.Organizer);
}

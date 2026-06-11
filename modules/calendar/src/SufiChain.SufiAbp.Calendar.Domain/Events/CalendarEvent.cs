using SufiChain.SufiAbp;
using SufiChain.SufiAbp.Domain.Entities.Auditing;
using SufiChain.SufiAbp.MultiTenancy;

namespace SufiChain.SufiAbp.Calendar.Events;

public class CalendarEvent : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; private set; }

    public virtual Guid CalendarId { get; private set; }

    public virtual string Title { get; private set; } = default!;

    public virtual DateTime StartUtc { get; private set; }

    public virtual DateTime EndUtc { get; private set; }

    public virtual bool IsAllDay { get; private set; }

    public virtual string TimeZoneId { get; private set; } = default!;

    public virtual string? Location { get; private set; }

    public virtual string? Description { get; private set; }

    public virtual string? Color { get; private set; }

    public virtual EventStatus Status { get; private set; }

    public virtual Guid? AvailabilityCalendarId { get; private set; }

    public virtual string? SourceType { get; private set; }

    public virtual string? SourceId { get; private set; }

    public virtual RecurrenceRule? RecurrenceRule { get; private set; }

    public virtual List<EventOccurrenceException> OccurrenceExceptions { get; private set; } = new();

    public virtual List<EventAttendee> Attendees { get; private set; } = new();

    public virtual List<EventReminder> Reminders { get; private set; } = new();

    protected CalendarEvent()
    {
    }

    public CalendarEvent(
        Guid id,
        Guid? tenantId,
        Guid calendarId,
        string title,
        DateTime startUtc,
        DateTime endUtc,
        bool isAllDay,
        string timeZoneId,
        EventStatus status = EventStatus.Confirmed,
        Guid? availabilityCalendarId = null,
        string? location = null,
        string? description = null,
        string? color = null,
        string? sourceType = null,
        string? sourceId = null)
        : base(id)
    {
        TenantId = tenantId;
        CalendarId = calendarId;
        SetTitle(title);
        SetTimeRange(startUtc, endUtc, isAllDay, timeZoneId);
        SetStatus(status);
        SetAvailabilityCalendar(availabilityCalendarId);
        SetDetails(location, description, color);
        AttachSource(sourceType, sourceId);
    }

    public virtual void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), EventConsts.MaxTitleLength);
    }

    public virtual void SetTimeRange(DateTime startUtc, DateTime endUtc, bool isAllDay, string timeZoneId)
    {
        if (endUtc <= startUtc)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidTimeRange);
        }

        Check.NotNullOrWhiteSpace(timeZoneId, nameof(timeZoneId), EventConsts.MaxTimeZoneIdLength);
        _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

        StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        EndUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
        IsAllDay = isAllDay;
        TimeZoneId = timeZoneId;
    }

    public virtual void SetDetails(string? location, string? description, string? color)
    {
        Location = ValidateOptionalLength(location, nameof(location), EventConsts.MaxLocationLength);
        Description = ValidateOptionalLength(description, nameof(description), EventConsts.MaxDescriptionLength);
        Color = ValidateOptionalLength(color, nameof(color), EventConsts.MaxColorLength);
    }

    public virtual void SetStatus(EventStatus status)
    {
        Status = status;
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void SetAvailabilityCalendar(Guid? availabilityCalendarId)
    {
        AvailabilityCalendarId = availabilityCalendarId;
    }

    public virtual void AttachSource(string? sourceType, string? sourceId)
    {
        if (string.IsNullOrWhiteSpace(sourceType) != string.IsNullOrWhiteSpace(sourceId))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidSource);
        }

        SourceType = ValidateOptionalLength(sourceType, nameof(sourceType), EventConsts.MaxSourceTypeLength);
        SourceId = ValidateOptionalLength(sourceId, nameof(sourceId), EventConsts.MaxSourceIdLength);
    }

    private static string? ValidateOptionalLength(string? value, string parameterName, int maxLength)
    {
        if (value is not null && value.Length > maxLength)
        {
            throw new ArgumentException($"{parameterName} can not be longer than {maxLength} characters.", parameterName);
        }

        return value;
    }

    public virtual void AddAttendee(EventAttendee attendee)
    {
        if (attendee.EventId != Id)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidAttendee);
        }

        if (attendee.Role == AttendeeRole.Organizer && Attendees.Any(x => x.Role == AttendeeRole.Organizer))
        {
            throw new BusinessException(CalendarErrorCodes.OrganizerRequired);
        }

        Attendees.Add(attendee);
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void RemoveAttendee(Guid attendeeId)
    {
        var attendee = Attendees.FirstOrDefault(x => x.Id == attendeeId);
        if (attendee is null)
        {
            return;
        }

        if (attendee.Role == AttendeeRole.Organizer && Attendees.Count(x => x.Role == AttendeeRole.Organizer) == 1)
        {
            throw new BusinessException(CalendarErrorCodes.OrganizerRequired);
        }

        Attendees.Remove(attendee);
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void SetRsvp(Guid attendeeId, RsvpStatus rsvpStatus)
    {
        var attendee = Attendees.FirstOrDefault(x => x.Id == attendeeId);
        if (attendee is null)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidAttendee);
        }

        attendee.SetRsvpStatus(rsvpStatus);
        AddDistributedEvent(new EventRsvpChangedEto(Id, CalendarId, attendeeId, rsvpStatus, TenantId));
    }

    public virtual void AddReminder(EventReminder reminder)
    {
        if (reminder.EventId != Id || (reminder.AttendeeId.HasValue && Attendees.All(x => x.Id != reminder.AttendeeId.Value)))
        {
            throw new BusinessException(CalendarErrorCodes.InvalidAttendee);
        }

        Reminders.Add(reminder);
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void RemoveReminder(Guid reminderId)
    {
        Reminders.RemoveAll(x => x.Id == reminderId);
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void SetRecurrence(Guid recurrenceRuleId, string rule)
    {
        RecurrenceRule = new RecurrenceRule(recurrenceRuleId, Id, rule);
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void ClearRecurrence()
    {
        RecurrenceRule = null;
        OccurrenceExceptions.Clear();
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void AddOrReplaceOccurrenceException(EventOccurrenceException occurrenceException)
    {
        if (RecurrenceRule is null)
        {
            throw new BusinessException(CalendarErrorCodes.EventNotRecurring);
        }

        if (occurrenceException.EventId != Id)
        {
            throw new BusinessException(CalendarErrorCodes.InvalidOccurrenceOverride);
        }

        OccurrenceExceptions.RemoveAll(x => x.OriginalStartUtc == occurrenceException.OriginalStartUtc);
        OccurrenceExceptions.Add(occurrenceException);
        AddDistributedEvent(new CalendarEventChangedEto(Id, CalendarId, TenantId));
    }

    public virtual void CancelOccurrence(Guid exceptionId, DateTime originalStartUtc, bool thisAndFollowing = false)
    {
        AddOrReplaceOccurrenceException(EventOccurrenceException.Cancel(exceptionId, Id, originalStartUtc, thisAndFollowing));
    }

    public virtual void MoveOccurrence(Guid exceptionId, DateTime originalStartUtc, DateTime movedStartUtc, DateTime movedEndUtc, bool thisAndFollowing = false)
    {
        AddOrReplaceOccurrenceException(EventOccurrenceException.Move(exceptionId, Id, originalStartUtc, movedStartUtc, movedEndUtc, thisAndFollowing));
    }
}

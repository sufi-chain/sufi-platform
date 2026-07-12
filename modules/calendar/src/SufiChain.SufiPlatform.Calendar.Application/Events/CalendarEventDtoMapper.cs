using SufiChain.SufiPlatform.Data;

namespace SufiChain.SufiPlatform.Calendar.Events;

public static class CalendarEventDtoMapper
{
    public static CalendarEventDto ToDto(CalendarEvent calendarEvent)
    {
        return new CalendarEventDto
        {
            Id = calendarEvent.Id,
            TenantId = calendarEvent.TenantId,
            CalendarId = calendarEvent.CalendarId,
            Title = calendarEvent.Title,
            StartUtc = calendarEvent.StartUtc,
            EndUtc = calendarEvent.EndUtc,
            IsAllDay = calendarEvent.IsAllDay,
            TimeZoneId = calendarEvent.TimeZoneId,
            Location = calendarEvent.Location,
            Description = calendarEvent.Description,
            Color = calendarEvent.Color,
            Status = calendarEvent.Status,
            AvailabilityCalendarId = calendarEvent.AvailabilityCalendarId,
            SourceType = calendarEvent.SourceType,
            SourceId = calendarEvent.SourceId,
            RecurrenceRule = calendarEvent.RecurrenceRule?.Rule,
            ExtraProperties = new ExtraPropertyDictionary(calendarEvent.ExtraProperties),
            Attendees = calendarEvent.Attendees.Select(ToDto).ToList(),
            Reminders = calendarEvent.Reminders.Select(ToDto).ToList()
        };
    }

    public static EventOccurrenceDto ToDto(EventOccurrence occurrence)
    {
        return new EventOccurrenceDto
        {
            EventId = occurrence.EventId,
            CalendarId = occurrence.CalendarId,
            Title = occurrence.Title,
            OriginalStartUtc = occurrence.OriginalStartUtc,
            StartUtc = occurrence.StartUtc,
            EndUtc = occurrence.EndUtc,
            IsAllDay = occurrence.IsAllDay,
            TimeZoneId = occurrence.TimeZoneId,
            Status = occurrence.Status,
            Location = occurrence.Location,
            Description = occurrence.Description,
            Color = occurrence.Color,
            SourceType = occurrence.SourceType,
            SourceId = occurrence.SourceId
        };
    }

    public static EventAttendeeDto ToDto(EventAttendee attendee)
    {
        return new EventAttendeeDto
        {
            Id = attendee.Id,
            EventId = attendee.EventId,
            UserId = attendee.UserId,
            Email = attendee.Email,
            DisplayName = attendee.DisplayName,
            Role = attendee.Role,
            RsvpStatus = attendee.RsvpStatus
        };
    }

    public static EventReminderDto ToDto(EventReminder reminder)
    {
        return new EventReminderDto
        {
            Id = reminder.Id,
            EventId = reminder.EventId,
            Offset = reminder.Offset,
            Channel = reminder.Channel,
            AttendeeId = reminder.AttendeeId,
            SentAtUtc = reminder.SentAtUtc
        };
    }
}

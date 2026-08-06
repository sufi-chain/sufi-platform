using SufiChain.SufiPlatform.Application.Dtos;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CalendarEventDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid CalendarId { get; set; }

    public string Title { get; set; } = string.Empty;

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public bool IsAllDay { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public string? Location { get; set; }

    public string? Description { get; set; }

    public string? Color { get; set; }

    public EventStatus Status { get; set; }

    public Guid? AvailabilityCalendarId { get; set; }

    public string? SourceType { get; set; }

    public string? SourceId { get; set; }

    public string? RecurrenceRule { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();

    public List<EventAttendeeDto> Attendees { get; set; } = new();

    public List<EventReminderDto> Reminders { get; set; } = new();
}

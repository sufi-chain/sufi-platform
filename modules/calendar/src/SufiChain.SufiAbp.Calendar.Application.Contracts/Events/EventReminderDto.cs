using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Calendar.Events;

public class EventReminderDto : EntityDto<Guid>
{
    public Guid EventId { get; set; }

    public TimeSpan Offset { get; set; }

    public ReminderChannel Channel { get; set; }

    public Guid? AttendeeId { get; set; }

    public DateTime? SentAtUtc { get; set; }
}

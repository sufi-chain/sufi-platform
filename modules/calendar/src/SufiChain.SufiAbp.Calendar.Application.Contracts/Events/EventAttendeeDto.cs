using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.Calendar.Events;

public class EventAttendeeDto : EntityDto<Guid>
{
    public Guid EventId { get; set; }

    public Guid? UserId { get; set; }

    [StringLength(EventConsts.MaxAttendeeEmailLength)]
    public string? Email { get; set; }

    [Required]
    [StringLength(EventConsts.MaxAttendeeDisplayNameLength)]
    public string DisplayName { get; set; } = string.Empty;

    public AttendeeRole Role { get; set; }

    public RsvpStatus RsvpStatus { get; set; }
}

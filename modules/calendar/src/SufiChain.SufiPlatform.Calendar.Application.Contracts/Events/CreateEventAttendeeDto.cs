using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CreateEventAttendeeDto
{
    public Guid? UserId { get; set; }

    [StringLength(EventConsts.MaxAttendeeEmailLength)]
    public string? Email { get; set; }

    [Required]
    [StringLength(EventConsts.MaxAttendeeDisplayNameLength)]
    public string DisplayName { get; set; } = string.Empty;

    public AttendeeRole Role { get; set; } = AttendeeRole.Required;
}

using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.Data;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CreateUpdateCalendarDto
{
    [Required]
    [StringLength(CalendarConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    public CalendarKind Kind { get; set; }

    [Required]
    [StringLength(CalendarConsts.MaxTimeZoneIdLength)]
    public string TimeZoneId { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string? OwnerName { get; set; }

    public bool IsDefault { get; set; }

    public bool IsAlwaysOpen { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();
}

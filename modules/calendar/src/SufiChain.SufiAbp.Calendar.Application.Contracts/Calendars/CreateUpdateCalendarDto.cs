using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class CreateUpdateCalendarDto
{
    [Required]
    [StringLength(CalendarConsts.MaxNameLength)]
    public string Name { get; set; } = string.Empty;

    public CalendarKind Kind { get; set; }

    [Required]
    [StringLength(CalendarConsts.MaxTimeZoneIdLength)]
    public string TimeZoneId { get; set; } = string.Empty;

    public CalendarOwnerType OwnerType { get; set; }

    public Guid? OwnerId { get; set; }

    public bool IsDefault { get; set; }

    public int? MaxConcurrent { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();
}

using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarLookupDto : EntityDto<Guid>
{
    public string Name { get; set; } = string.Empty;

    public CalendarKind Kind { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string? OwnerName { get; set; }

    public bool IsDefault { get; set; }
}

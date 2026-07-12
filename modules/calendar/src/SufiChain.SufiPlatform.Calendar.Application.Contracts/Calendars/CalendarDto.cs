using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Data;

namespace SufiChain.SufiPlatform.Calendar.Calendars;

public class CalendarDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CalendarKind Kind { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public Guid? OwnerUserId { get; set; }

    public string? OwnerName { get; set; }

    public bool IsDefault { get; set; }

    public bool IsAlwaysOpen { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();

    public List<WorkingHourRuleDto> WorkingHourRules { get; set; } = new();

    public List<CalendarExceptionDto> Exceptions { get; set; } = new();

    public List<CalendarInheritanceDto> Inheritances { get; set; } = new();
}

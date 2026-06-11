using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Data;

namespace SufiChain.SufiAbp.Calendar.Calendars;

public class CalendarDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string Name { get; set; } = string.Empty;

    public CalendarKind Kind { get; set; }

    public string TimeZoneId { get; set; } = string.Empty;

    public CalendarOwnerType OwnerType { get; set; }

    public Guid? OwnerId { get; set; }

    public bool IsDefault { get; set; }

    public int? MaxConcurrent { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();

    public List<WorkingHourRuleDto> WorkingHourRules { get; set; } = new();

    public List<CalendarExceptionDto> Exceptions { get; set; } = new();
}

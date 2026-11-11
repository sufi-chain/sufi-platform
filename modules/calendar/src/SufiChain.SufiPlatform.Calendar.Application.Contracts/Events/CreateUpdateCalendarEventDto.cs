using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Calendar.Localization;
using Volo.Abp.Data;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CreateUpdateCalendarEventDto : IValidatableObject
{
    public Guid CalendarId { get; set; }

    [Required]
    [StringLength(EventConsts.MaxTitleLength)]
    public string Title { get; set; } = string.Empty;

    public DateTime StartUtc { get; set; }

    public DateTime EndUtc { get; set; }

    public bool IsAllDay { get; set; }

    [StringLength(EventConsts.MaxTimeZoneIdLength)]
    public string? TimeZoneId { get; set; }

    [StringLength(EventConsts.MaxLocationLength)]
    public string? Location { get; set; }

    [StringLength(EventConsts.MaxDescriptionLength)]
    public string? Description { get; set; }

    [StringLength(EventConsts.MaxColorLength)]
    public string? Color { get; set; }

    public EventStatus Status { get; set; } = EventStatus.Confirmed;

    public Guid? AvailabilityCalendarId { get; set; }

    [StringLength(EventConsts.MaxSourceTypeLength)]
    public string? SourceType { get; set; }

    [StringLength(EventConsts.MaxSourceIdLength)]
    public string? SourceId { get; set; }

    [StringLength(EventConsts.MaxRecurrenceRuleLength)]
    public string? RecurrenceRule { get; set; }

    public ExtraPropertyDictionary ExtraProperties { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndUtc > StartUtc)
        {
            yield break;
        }

        var localizer = validationContext.GetRequiredService<IStringLocalizer<CalendarResource>>();
        var message = localizer["InvalidTimeRange"].Value;
        yield return new ValidationResult(message, new[] { nameof(StartUtc), nameof(EndUtc) });
    }
}

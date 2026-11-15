using System.ComponentModel.DataAnnotations;
using System.Globalization;
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

    /// <summary>Repeat every must be at least 1. A missing INTERVAL stays the default of 1.</summary>
    private static readonly RangeAttribute RecurrenceIntervalRange = new(1, int.MaxValue);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var localizer = validationContext.GetRequiredService<IStringLocalizer<CalendarResource>>();

        if (EndUtc <= StartUtc)
        {
            var message = localizer["InvalidTimeRange"].Value;
            yield return new ValidationResult(message, new[] { nameof(StartUtc), nameof(EndUtc) });
        }

        if (HasInvalidRecurrenceInterval(RecurrenceRule))
        {
            var message = localizer["InvalidRecurrenceInterval"].Value;
            yield return new ValidationResult(message, new[] { nameof(RecurrenceRule) });
        }
    }

    private static bool HasInvalidRecurrenceInterval(string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
        {
            return false;
        }

        foreach (var part in rule.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var pieces = part.Split('=', 2, StringSplitOptions.TrimEntries);
            if (pieces.Length != 2 || !pieces[0].Equals("INTERVAL", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            return !int.TryParse(pieces[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var interval)
                || !RecurrenceIntervalRange.IsValid(interval);
        }

        return false;
    }
}

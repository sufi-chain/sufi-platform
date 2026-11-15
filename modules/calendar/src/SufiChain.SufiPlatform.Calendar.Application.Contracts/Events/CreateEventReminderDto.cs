using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Calendar.Localization;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CreateEventReminderDto : IValidatableObject
{
    /// <summary>
    /// Minutes before the event, stored as a negative offset. Zero is at the start.
    /// A positive offset is minutes after the event and is rejected.
    /// </summary>
    public TimeSpan Offset { get; set; }

    public ReminderChannel Channel { get; set; }

    public Guid? AttendeeId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var earliest = TimeSpan.FromDays(-EventConsts.MaxReminderOffsetDays);
        if (Offset <= TimeSpan.Zero && Offset >= earliest)
        {
            yield break;
        }

        var localizer = validationContext.GetRequiredService<IStringLocalizer<CalendarResource>>();
        var message = localizer["InvalidReminderOffset"].Value;
        yield return new ValidationResult(message, new[] { nameof(Offset) });
    }
}

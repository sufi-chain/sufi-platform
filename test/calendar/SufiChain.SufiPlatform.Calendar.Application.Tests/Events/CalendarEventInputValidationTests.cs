using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Events;

public class CalendarEventInputValidationTests
{
    private static readonly DateTime Start = new(2026, 9, 8, 9, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData("FREQ=DAILY;INTERVAL=0")]
    [InlineData("FREQ=WEEKLY;INTERVAL=-5")]
    [InlineData("freq=daily;interval=0")]
    public void Recurrence_Interval_Below_One_Is_A_Localized_Range_Error(string rule)
    {
        var dto = ValidEvent();
        dto.RecurrenceRule = rule;

        var result = Validate(dto).Single();

        result.ErrorMessage.ShouldBe("Repeat every must be at least 1.");
        result.MemberNames.ShouldBe(new[] { nameof(CreateUpdateCalendarEventDto.RecurrenceRule) });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("FREQ=DAILY")]
    [InlineData("FREQ=DAILY;INTERVAL=1")]
    public void Missing_Or_Positive_Interval_Is_Accepted(string? rule)
    {
        var dto = ValidEvent();
        dto.RecurrenceRule = rule;

        Validate(dto).ShouldBeEmpty();
    }

    [Fact]
    public void Negative_Minutes_Before_Is_A_Localized_Error()
    {
        var dto = new CreateEventReminderDto
        {
            Offset = TimeSpan.FromMinutes(5),
            Channel = ReminderChannel.Email
        };

        var result = Validate(dto).Single();

        result.ErrorMessage.ShouldBe("Minutes before must be at least 0 and no more than 30 days.");
        result.MemberNames.ShouldBe(new[] { nameof(CreateEventReminderDto.Offset) });
    }

    [Fact]
    public void Reminder_Farther_Than_Thirty_Days_Is_Rejected()
    {
        var dto = new CreateEventReminderDto
        {
            Offset = TimeSpan.FromDays(-(EventConsts.MaxReminderOffsetDays + 1)),
            Channel = ReminderChannel.InApp
        };

        Validate(dto).Single().MemberNames.ShouldBe(new[] { nameof(CreateEventReminderDto.Offset) });
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Minutes_Before_Inside_The_Cap_Is_Accepted(int minutes)
    {
        var dto = new CreateEventReminderDto
        {
            Offset = TimeSpan.FromMinutes(-minutes),
            Channel = ReminderChannel.Email
        };

        Validate(dto).ShouldBeEmpty();
    }

    private static CreateUpdateCalendarEventDto ValidEvent() => new()
    {
        Title = "Planning",
        StartUtc = Start,
        EndUtc = Start.AddHours(1),
        TimeZoneId = null
    };

    private static List<ValidationResult> Validate(object dto)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IStringLocalizer<CalendarResource>, CalendarText>();
        var context = new ValidationContext(dto, services.BuildServiceProvider(), null);
        return ((IValidatableObject)dto).Validate(context).ToList();
    }

    private sealed class CalendarText : IStringLocalizer<CalendarResource>
    {
        public LocalizedString this[string name] => new(name, Text(name));

        public LocalizedString this[string name, params object[] arguments] =>
            new(name, string.Format(CultureInfo.InvariantCulture, Text(name), arguments));

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) =>
            Array.Empty<LocalizedString>();

        private static string Text(string name) => name switch
        {
            "InvalidRecurrenceInterval" => "Repeat every must be at least 1.",
            "InvalidReminderOffset" => "Minutes before must be at least 0 and no more than 30 days.",
            "InvalidTimeRange" => "Invalid time range",
            _ => name
        };
    }
}

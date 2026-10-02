using System.Globalization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Pages;

public partial class CalendarEventPageBase : CalendarPublicComponentBase
{
    [Parameter]
    public Guid? EventId { get; set; }

    [SupplyParameterFromQuery(Name = "calendarId")]
    public Guid? CalendarIdQuery { get; set; }

    [SupplyParameterFromQuery(Name = "start")]
    public string? StartQuery { get; set; }

    [SupplyParameterFromQuery(Name = "end")]
    public string? EndQuery { get; set; }

    [SupplyParameterFromQuery(Name = "timeZone")]
    public string? TimeZoneQuery { get; set; }

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    [Inject]
    protected IPageLayout PageLayout { get; set; } = default!;

    private string? _appliedTitle;

    protected Guid CalendarId => CalendarIdQuery ?? Guid.Empty;

    protected DateTime? InitialStartUtc => TryParseUtc(StartQuery);

    protected DateTime? InitialEndUtc => TryParseUtc(EndQuery);

    protected string TimeZoneId => string.IsNullOrWhiteSpace(TimeZoneQuery) ? TimeZoneInfo.Local.Id : TimeZoneQuery;

    protected override void OnParametersSet()
    {
        var title = EventId.HasValue ? L["EditEvent"].Value : L["CreateEvent"].Value;
        if (string.Equals(_appliedTitle, title, StringComparison.Ordinal))
        {
            return;
        }

        _appliedTitle = title;
        PageLayout.Title = title;
    }

    private static DateTime? TryParseUtc(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        if (!DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
        {
            return null;
        }

        return parsed.Kind == DateTimeKind.Utc
            ? parsed
            : DateTime.SpecifyKind(parsed, DateTimeKind.Utc);
    }
}

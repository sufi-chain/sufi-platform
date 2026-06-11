using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using System.Globalization;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public.Components;

public partial class SufiCalendarView : CalendarPublicComponentBase
{
    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Parameter]
    public SufiCalendarViewMode View { get; set; } = SufiCalendarViewMode.Month;

    [Parameter]
    public EventCallback<SufiCalendarViewMode> ViewChanged { get; set; }

    [Parameter]
    public DateTime Date { get; set; } = DateTime.Today;

    [Parameter]
    public EventCallback<DateTime> DateChanged { get; set; }

    [Parameter]
    public IReadOnlyList<Guid> CalendarIds { get; set; } = Array.Empty<Guid>();

    [Parameter]
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    [Parameter]
    public EventCallback<EventOccurrenceDto> OnEventClick { get; set; }

    [Parameter]
    public EventCallback<SufiCalendarSlotSelectArgs> OnSlotSelect { get; set; }

    [Parameter]
    public int MaxEventsPerDay { get; set; } = 3;

    private readonly List<EventOccurrenceDto> _visibleOccurrences = new();
    private List<DateTime> _days = new();
    private string[] _dayNames = Array.Empty<string>();
    private CultureInfo _culture = CultureInfo.CurrentUICulture;
    private bool _isLoading;

    protected string Direction => SbCalendarHelper.IsRtl(SbCalendarHelper.GetCalendarSystemFromCulture(_culture)) ? "rtl" : "ltr";

    protected string PreviousLabel => Direction == "rtl" ? L["Next"] : L["Previous"];

    protected string NextLabel => Direction == "rtl" ? L["Previous"] : L["Next"];

    protected string TitleText => $"{SbCalendarHelper.GetMonthName(Date, _culture)} {SbCalendarHelper.GetYear(Date, _culture)}";

    protected override async Task OnParametersSetAsync()
    {
        _culture = CultureInfo.CurrentUICulture;
        BuildVisibleDays();
        await LoadOccurrencesAsync();
    }

    protected virtual async Task SetViewAsync(SufiCalendarViewMode mode)
    {
        View = mode;
        await ViewChanged.InvokeAsync(mode);
        BuildVisibleDays();
        await LoadOccurrencesAsync();
    }

    protected virtual async Task PreviousAsync()
    {
        Date = View switch
        {
            SufiCalendarViewMode.Day => Date.AddDays(-1),
            SufiCalendarViewMode.Week => Date.AddDays(-7),
            _ => Date.AddMonths(-1)
        };

        await DateChanged.InvokeAsync(Date);
        BuildVisibleDays();
        await LoadOccurrencesAsync();
    }

    protected virtual async Task NextAsync()
    {
        Date = View switch
        {
            SufiCalendarViewMode.Day => Date.AddDays(1),
            SufiCalendarViewMode.Week => Date.AddDays(7),
            _ => Date.AddMonths(1)
        };

        await DateChanged.InvokeAsync(Date);
        BuildVisibleDays();
        await LoadOccurrencesAsync();
    }

    protected virtual async Task SelectEventAsync(EventOccurrenceDto occurrence)
    {
        await OnEventClick.InvokeAsync(occurrence);
    }

    protected virtual async Task SelectSlotAsync(DateTime localDay)
    {
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDay.Date, DateTimeKind.Unspecified), ResolveTimeZone());
        await OnSlotSelect.InvokeAsync(new SufiCalendarSlotSelectArgs(startUtc, startUtc.AddDays(1), CalendarIds));
    }

    protected virtual IEnumerable<EventOccurrenceDto> GetOccurrencesForDay(DateTime day)
    {
        var timeZone = ResolveTimeZone();
        return _visibleOccurrences.Where(x => TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(x.StartUtc, DateTimeKind.Utc), timeZone).Date == day.Date);
    }

    protected virtual string FormatDateTime(DateTime utc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(utc, DateTimeKind.Utc), ResolveTimeZone());
        return SbCalendarHelper.FormatDate(local, null, _culture) + " " + local.ToString("HH:mm", _culture);
    }

    protected virtual string GetDayClass(DateTime day)
    {
        var currentMonth = SbCalendarHelper.GetMonth(day, _culture) == SbCalendarHelper.GetMonth(Date, _culture) &&
                           SbCalendarHelper.GetYear(day, _culture) == SbCalendarHelper.GetYear(Date, _culture);
        return currentMonth ? "sufi-calendar-view__day" : "sufi-calendar-view__day sufi-calendar-view__day--muted";
    }

    private void BuildVisibleDays()
    {
        var firstDay = _culture.DateTimeFormat.FirstDayOfWeek;
        _dayNames = SbCalendarHelper.GetDayNames(_culture, firstDay);
        _days = View switch
        {
            SufiCalendarViewMode.Day => new List<DateTime> { Date.Date },
            SufiCalendarViewMode.Week => SbCalendarHelper.GetWeekDays(Date.Date, firstDay).ToList(),
            _ => SbCalendarHelper.GetMonthViewDays(Date.Date, _culture, firstDay).ToList()
        };
    }

    private async Task LoadOccurrencesAsync()
    {
        _visibleOccurrences.Clear();
        if (CalendarIds.Count == 0 || _days.Count == 0)
        {
            return;
        }

        _isLoading = true;
        var fromUtc = ToUtc(_days.Min().Date);
        var toUtc = ToUtc(_days.Max().Date.AddDays(1));

        foreach (var calendarId in CalendarIds.Distinct())
        {
            var result = await CalendarEventAppService.GetOccurrencesAsync(calendarId, new GetOccurrencesInput
            {
                FromUtc = fromUtc,
                ToUtc = toUtc
            });
            _visibleOccurrences.AddRange(result.Items);
        }

        _visibleOccurrences.Sort((left, right) => left.StartUtc.CompareTo(right.StartUtc));
        _isLoading = false;
    }

    private DateTime ToUtc(DateTime local)
    {
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), ResolveTimeZone());
    }

    private TimeZoneInfo ResolveTimeZone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }
}

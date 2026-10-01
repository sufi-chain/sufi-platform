using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using System.Globalization;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class SufiCalendarView : CalendarPublicComponentBase
{
    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

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

    /// <summary>
    /// Retained for callers; month/week cells show one count badge per calendar.
    /// </summary>
    [Parameter]
    public int MaxEventsPerDay { get; set; } = 3;

    /// <summary>
    /// Optional compact chrome in the calendar toolbar, after the month title
    /// (e.g. Pro calendar selector chip).
    /// </summary>
    [Parameter]
    public RenderFragment? ToolbarExtra { get; set; }

    private readonly List<EventOccurrenceDto> _visibleOccurrences = new();
    private readonly HashSet<DateOnly> _closedDays = new();
    private List<DateTime> _days = new();
    private string[] _dayNames = Array.Empty<string>();
    private CultureInfo _culture = CultureInfo.CurrentUICulture;
    private bool _isLoading;
    private int _loadVersion;
    private string? _lastLoadKey;
    private DateTime? _eventsPopoverDay;
    private Guid? _eventsPopoverCalendarId;

    protected string Direction => SbCalendarHelper.IsRtl(SbCalendarHelper.GetCalendarSystemFromCulture(_culture)) ? "rtl" : "ltr";

    protected string PreviousLabel => Direction == "rtl" ? L["Next"] : L["Previous"];

    protected string NextLabel => Direction == "rtl" ? L["Previous"] : L["Next"];

    protected string TitleText => $"{SbCalendarHelper.GetMonthName(Date, _culture)} {SbCalendarHelper.GetYear(Date, _culture)}";

    protected IEnumerable<int> DayTimelineHours => Enumerable.Range(0, 24);

    protected override async Task OnParametersSetAsync()
    {
        _culture = CultureInfo.CurrentUICulture;
        BuildVisibleDays();
        await LoadOccurrencesAsync();
    }

    protected virtual async Task SetViewAsync(SufiCalendarViewMode mode)
    {
        CloseEventsPopover();
        View = mode;
        await ViewChanged.InvokeAsync(mode);
        BuildVisibleDays();
        await LoadOccurrencesAsync();
    }

    protected virtual async Task PreviousAsync()
    {
        CloseEventsPopover();
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
        CloseEventsPopover();
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

    protected virtual async Task GoToTodayAsync()
    {
        CloseEventsPopover();
        Date = GetTodayLocalDate();
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
        CloseEventsPopover();
        var startUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(localDay.Date, DateTimeKind.Unspecified), ResolveTimeZone());
        await OnSlotSelect.InvokeAsync(new SufiCalendarSlotSelectArgs(startUtc, startUtc.AddDays(1), CalendarIds));
    }

    protected bool IsDayEventsPopoverOpen(DateTime day) =>
        _eventsPopoverDay.HasValue && _eventsPopoverDay.Value.Date == day.Date;

    protected bool IsEventsPopoverOpen(DateTime day, Guid calendarId) =>
        IsDayEventsPopoverOpen(day) && _eventsPopoverCalendarId == calendarId;

    protected Task SetEventsPopoverOpenAsync(DateTime day, Guid calendarId, bool open)
    {
        if (!open)
        {
            if (!IsEventsPopoverOpen(day, calendarId))
            {
                return Task.CompletedTask;
            }

            _eventsPopoverDay = null;
            _eventsPopoverCalendarId = null;
            return InvokeAsync(StateHasChanged);
        }

        var nextDay = day.Date;
        if (_eventsPopoverDay == nextDay && _eventsPopoverCalendarId == calendarId)
        {
            return Task.CompletedTask;
        }

        _eventsPopoverDay = nextDay;
        _eventsPopoverCalendarId = calendarId;
        return InvokeAsync(StateHasChanged);
    }

    protected Task ToggleEventsPopoverAsync(DateTime day, Guid calendarId)
    {
        return SetEventsPopoverOpenAsync(day, calendarId, !IsEventsPopoverOpen(day, calendarId));
    }

    /// <summary>
    /// One bullet tag per calendar on that day, in first-occurrence order.
    /// Inherited calendars keep their own color and count instead of folding into the selected calendar.
    /// </summary>
    protected virtual IReadOnlyList<DayCalendarEventGroup> GetDayEventGroups(IEnumerable<EventOccurrenceDto> occurrences)
    {
        return occurrences
            .GroupBy(occurrence => occurrence.CalendarId)
            .Select(group =>
            {
                var items = group.ToList();
                var color = items.Select(item => item.Color).FirstOrDefault(itemColor => !string.IsNullOrWhiteSpace(itemColor));
                return new DayCalendarEventGroup(group.Key, color, items);
            })
            .ToList();
    }

    protected virtual string? GetGroupAccentStyle(DayCalendarEventGroup group)
    {
        return string.IsNullOrWhiteSpace(group.Color) ? null : $"--sufi-event-color:{group.Color}";
    }

    protected async Task SelectEventFromPopoverAsync(EventOccurrenceDto occurrence)
    {
        CloseEventsPopover();
        await SelectEventAsync(occurrence);
    }

    protected async Task HandleDayKeyDownAsync(KeyboardEventArgs e, DateTime day)
    {
        if (e.Key is "Enter" or " ")
        {
            await SelectSlotAsync(day);
        }
    }

    protected async Task HandleEventCountKeyDownAsync(KeyboardEventArgs e, DateTime day, Guid calendarId)
    {
        if (e.Key is "Enter" or " ")
        {
            await ToggleEventsPopoverAsync(day, calendarId);
        }
    }

    private void CloseEventsPopover()
    {
        _eventsPopoverDay = null;
        _eventsPopoverCalendarId = null;
    }

    protected virtual async Task SelectHourSlotAsync(int hour)
    {
        var localStart = Date.Date.AddHours(hour);
        var startUtc = ToUtc(localStart);
        await OnSlotSelect.InvokeAsync(new SufiCalendarSlotSelectArgs(startUtc, startUtc.AddHours(1), CalendarIds));
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

    protected virtual string FormatHour(int hour)
    {
        return Date.Date.AddHours(hour).ToString("HH:mm", _culture);
    }

    protected virtual string FormatTimeRange(EventOccurrenceDto occurrence)
    {
        if (IsDisplayedAsAllDay(occurrence))
        {
            return L["AllDay"];
        }

        var timeZone = ResolveTimeZone();
        var start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurrence.StartUtc, DateTimeKind.Utc), timeZone);
        var end = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurrence.EndUtc, DateTimeKind.Utc), timeZone);
        return $"{start.ToString("HH:mm", _culture)} - {end.ToString("HH:mm", _culture)}";
    }

    protected virtual IEnumerable<EventOccurrenceDto> GetAllDayOccurrences()
    {
        var timeZone = ResolveTimeZone();
        var dayStart = Date.Date;
        var dayEnd = dayStart.AddDays(1);

        return _visibleOccurrences.Where(occurrence =>
        {
            if (!IsDisplayedAsAllDay(occurrence))
            {
                return false;
            }

            var (start, end) = GetLocalRange(occurrence, timeZone);
            return start < dayEnd && end > dayStart;
        });
    }

    protected virtual IEnumerable<EventOccurrenceDto> GetOccurrencesForHour(int hour)
    {
        var timeZone = ResolveTimeZone();
        var dayStart = Date.Date;
        var hourStart = dayStart.AddHours(hour);
        var hourEnd = hourStart.AddHours(1);

        return _visibleOccurrences.Where(occurrence =>
        {
            if (IsDisplayedAsAllDay(occurrence))
            {
                return false;
            }

            var (start, end) = GetLocalRange(occurrence, timeZone);
            if (!(start < hourEnd && end > hourStart))
            {
                return false;
            }

            // List-per-hour UI: render a timed event once (start hour, or 00:00 if it continues from a prior day).
            var displayHour = start.Date < dayStart ? 0 : start.Hour;
            return hour == displayHour;
        });
    }

    /// <summary>
    /// All-day flag, or a day-long span (common when seed/import stores midnight→midnight UTC).
    /// </summary>
    protected virtual bool IsDisplayedAsAllDay(EventOccurrenceDto occurrence)
    {
        if (occurrence.IsAllDay)
        {
            return true;
        }

        var (start, end) = GetLocalRange(occurrence, ResolveTimeZone());
        var duration = end - start;
        return duration >= TimeSpan.FromHours(23) && start.TimeOfDay == end.TimeOfDay;
    }

    private static (DateTime Start, DateTime End) GetLocalRange(EventOccurrenceDto occurrence, TimeZoneInfo timeZone)
    {
        var start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurrence.StartUtc, DateTimeKind.Utc), timeZone);
        var end = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(occurrence.EndUtc, DateTimeKind.Utc), timeZone);
        return (start, end);
    }

    protected virtual bool IsClosedDay(DateTime day) =>
        _closedDays.Contains(DateOnly.FromDateTime(day.Date));

    protected virtual string GetDayClass(DateTime day)
    {
        var currentMonth = SbCalendarHelper.GetMonth(day, _culture) == SbCalendarHelper.GetMonth(Date, _culture) &&
                           SbCalendarHelper.GetYear(day, _culture) == SbCalendarHelper.GetYear(Date, _culture);
        var classes = currentMonth
            ? "sufi-calendar-view__day"
            : "sufi-calendar-view__day sufi-calendar-view__day--muted";

        if (IsClosedDay(day))
        {
            classes += " sufi-calendar-view__day--closed";
        }

        if (day.Date == GetTodayLocalDate())
        {
            classes += " sufi-calendar-view__day--today";
        }

        if (IsDayEventsPopoverOpen(day))
        {
            classes += " sufi-calendar-view__day--events-open";
        }

        return classes;
    }

    protected virtual string? GetEventStyle(EventOccurrenceDto occurrence)
    {
        if (string.IsNullOrWhiteSpace(occurrence.Color))
        {
            return null;
        }

        // Color only — fill/text contrast is theme-aware in calendar-public.css.
        return $"--sufi-event-color:{occurrence.Color}";
    }

    protected virtual string GetGridClass()
    {
        var classes = new List<string> { "sufi-calendar-view__grid", $"sufi-calendar-view__grid--{View.ToString().ToLowerInvariant()}" };
        if (View == SufiCalendarViewMode.Month)
        {
            classes.Add(_days.Count > 35 ? "sufi-calendar-view__grid--six-rows" : "sufi-calendar-view__grid--five-rows");
        }

        return string.Join(" ", classes);
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

    private string BuildLoadKey()
    {
        var calendarKey = string.Join(",", CalendarIds.Distinct().OrderBy(x => x));
        var from = _days.Count == 0 ? string.Empty : _days.Min().ToString("yyyyMMdd");
        var to = _days.Count == 0 ? string.Empty : _days.Max().ToString("yyyyMMdd");
        return $"{calendarKey}|{View}|{Date:yyyyMMdd}|{TimeZoneId}|{from}|{to}";
    }

    private async Task LoadOccurrencesAsync()
    {
        var loadKey = BuildLoadKey();
        if (loadKey == _lastLoadKey && !_isLoading)
        {
            return;
        }

        var version = ++_loadVersion;

        if (CalendarIds.Count == 0 || _days.Count == 0)
        {
            if (version != _loadVersion)
            {
                return;
            }

            _visibleOccurrences.Clear();
            _closedDays.Clear();
            _lastLoadKey = loadKey;
            _isLoading = false;
            return;
        }

        _isLoading = true;
        var rangeStart = DateOnly.FromDateTime(_days.Min().Date);
        var rangeEnd = DateOnly.FromDateTime(_days.Max().Date);
        var fromUtc = ToUtc(_days.Min().Date);
        var toUtc = ToUtc(_days.Max().Date.AddDays(1));

        var items = new List<EventOccurrenceDto>();
        var closed = new HashSet<DateOnly>();
        foreach (var calendarId in CalendarIds.Distinct())
        {
            var result = await CalendarEventAppService.GetOccurrencesAsync(calendarId, new GetOccurrencesInput
            {
                FromUtc = fromUtc,
                ToUtc = toUtc
            });

            if (version != _loadVersion)
            {
                return;
            }

            items.AddRange(result.Items);

            var exceptions = await AvailabilityCalendarAppService.GetEffectiveExceptionsAsync(calendarId);
            if (version != _loadVersion)
            {
                return;
            }

            foreach (var exception in exceptions.Items)
            {
                if (exception.Kind != CalendarExceptionKind.Closed)
                {
                    continue;
                }

                var closedDate = DateOnly.FromDateTime(exception.Date.Date);
                if (closedDate >= rangeStart && closedDate <= rangeEnd)
                {
                    closed.Add(closedDate);
                }
            }
        }

        if (version != _loadVersion)
        {
            return;
        }

        items.Sort((left, right) => left.StartUtc.CompareTo(right.StartUtc));
        _visibleOccurrences.Clear();
        _visibleOccurrences.AddRange(items);
        _closedDays.Clear();
        foreach (var day in closed)
        {
            _closedDays.Add(day);
        }

        _lastLoadKey = loadKey;
        _isLoading = false;
    }

    private DateTime ToUtc(DateTime local)
    {
        return TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(local, DateTimeKind.Unspecified), ResolveTimeZone());
    }

    private DateTime GetTodayLocalDate()
    {
        try
        {
            return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ResolveTimeZone()).Date;
        }
        catch (TimeZoneNotFoundException)
        {
            return DateTime.Today;
        }
        catch (InvalidTimeZoneException)
        {
            return DateTime.Today;
        }
    }

    private TimeZoneInfo ResolveTimeZone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
    }

    protected sealed record DayCalendarEventGroup(Guid CalendarId, string? Color, IReadOnlyList<EventOccurrenceDto> Occurrences);
}

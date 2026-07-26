using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Availability;
using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class SufiCalendarSelect : CalendarPublicComponentBase
{
    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

    [Parameter]
    public Guid SelectedCalendarId { get; set; }

    [Parameter]
    public EventCallback<Guid> SelectedCalendarIdChanged { get; set; }

    [Parameter]
    public EventCallback<CalendarLookupDto> SelectedCalendarChanged { get; set; }

    [Parameter]
    public Guid? InitialCalendarId { get; set; }

    [Parameter]
    public IReadOnlyList<CalendarLookupDto>? Calendars { get; set; }

    /// <summary>
    /// Label text displayed above the calendar selector (form contexts).
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Placeholder when no calendar is selected. Defaults to the Calendar localization key.
    /// </summary>
    [Parameter]
    public string? Placeholder { get; set; }

    /// <summary>
    /// When true, marks the field as required in form layouts.
    /// </summary>
    [Parameter]
    public bool Required { get; set; }

    /// <summary>
    /// Disables calendar selection.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Stretches the selector to the full width of its container (dialogs, forms).
    /// </summary>
    [Parameter]
    public bool FullWidth { get; set; }

    /// <summary>
    /// Optional additional CSS class names for the wrapper element.
    /// </summary>
    [Parameter]
    public string? Class { get; set; }

    protected List<CalendarLookupDto> CalendarOptions { get; set; } = new();
    protected string? SelectedCalendarKey { get; set; }
    protected bool IsLoaded { get; set; }
    private bool _hasAppliedInitialCalendarId;

    protected override async Task OnParametersSetAsync()
    {
        if (CalendarOptions.Count == 0)
        {
            await LoadCalendarsAsync();
        }

        await TryApplyInitialCalendarIdAsync();

        if (CalendarOptions.Count > 0)
        {
            SyncSelectedKey();
        }
    }

    public virtual async Task RefreshAsync()
    {
        IsLoaded = false;
        _hasAppliedInitialCalendarId = false;
        CalendarOptions.Clear();
        await LoadCalendarsAsync();
    }

    protected virtual async Task OnSelectedCalendarChangedAsync(string? value)
    {
        var selected = CalendarOptions.FirstOrDefault(x => GetCalendarKey(x) == value);
        if (selected == null)
        {
            return;
        }

        SelectedCalendarId = selected.Id;
        SelectedCalendarKey = GetCalendarKey(selected);
        await SelectedCalendarIdChanged.InvokeAsync(selected.Id);
        await SelectedCalendarChanged.InvokeAsync(selected);
    }

    protected virtual string GetCalendarOptionText(CalendarLookupDto calendar)
    {
        return $"{calendar.Name} · {L[$"Enum:CalendarKind:{calendar.Kind}"]}";
    }

    protected virtual string GetCalendarKey(CalendarLookupDto calendar)
    {
        return calendar.Id.ToString("N");
    }

    protected string EffectivePlaceholder => string.IsNullOrWhiteSpace(Placeholder) ? L["Calendar"].Value : Placeholder;

    protected string GetWrapperClass()
    {
        var classes = new List<string> { "sufi-calendar-select", "sufi-calendar-scheduler__calendar-select" };
        if (FullWidth)
        {
            classes.Add("sufi-calendar-select--full-width");
        }

        if (!string.IsNullOrWhiteSpace(Class))
        {
            classes.Add(Class);
        }

        return string.Join(' ', classes);
    }

    protected virtual async Task LoadCalendarsAsync()
    {
        CalendarOptions.Clear();

        if (Calendars is { Count: > 0 })
        {
            CalendarOptions.AddRange(Calendars);
        }
        else
        {
            var personalCalendar = await AvailabilityCalendarAppService.GetOrCreateMyPersonalCalendarAsync();
            CalendarOptions.Add(new CalendarLookupDto
            {
                Id = personalCalendar.Id,
                Name = personalCalendar.Name,
                Kind = personalCalendar.Kind,
                TimeZoneId = personalCalendar.TimeZoneId,
                OwnerUserId = personalCalendar.OwnerUserId,
                OwnerName = personalCalendar.OwnerName,
                IsDefault = personalCalendar.IsDefault,
                Color = personalCalendar.Color
            });

            var visibleCalendars = await AvailabilityCalendarAppService.GetMyVisibleCalendarsAsync();
            CalendarOptions.AddRange(visibleCalendars.Items.Where(x => x.Id != personalCalendar.Id));
        }

        var selected = CalendarSelectionHelper.ResolveDefaultSelection(
            CalendarOptions,
            SelectedCalendarId,
            InitialCalendarId);

        if (selected != null && SelectedCalendarId == Guid.Empty)
        {
            SelectedCalendarId = selected.Id;
            SelectedCalendarKey = GetCalendarKey(selected);
            await SelectedCalendarIdChanged.InvokeAsync(selected.Id);
            await SelectedCalendarChanged.InvokeAsync(selected);
        }
        else
        {
            SyncSelectedKey();
        }

        IsLoaded = true;
    }

    private async Task TryApplyInitialCalendarIdAsync()
    {
        if (_hasAppliedInitialCalendarId || !InitialCalendarId.HasValue || CalendarOptions.Count == 0)
        {
            return;
        }

        var initial = CalendarOptions.FirstOrDefault(x => x.Id == InitialCalendarId.Value);
        if (initial == null)
        {
            return;
        }

        if (SelectedCalendarId != initial.Id)
        {
            SelectedCalendarId = initial.Id;
            SelectedCalendarKey = GetCalendarKey(initial);
            await SelectedCalendarIdChanged.InvokeAsync(initial.Id);
            await SelectedCalendarChanged.InvokeAsync(initial);
        }

        _hasAppliedInitialCalendarId = true;
    }

    private void SyncSelectedKey()
    {
        var selected = CalendarOptions.FirstOrDefault(x => x.Id == SelectedCalendarId);
        if (selected != null)
        {
            SelectedCalendarKey = GetCalendarKey(selected);
        }
    }
}

using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Calendar.Calendars;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare.Blazor.Components;

public partial class ProCalendarSelector : CalendarHooshvareComponentBase
{
    [Inject]
    protected ICalendarCatalogIntegrationService CalendarCatalog { get; set; } = default!;

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

    [Parameter]
    public bool Disabled { get; set; }

    protected List<CalendarLookupDto> CalendarOptions { get; set; } = new();

    protected CalendarLookupDto? SelectedCalendar { get; set; }

    protected bool IsLoaded { get; set; }

    protected bool IsOpen { get; set; }

    protected string TriggerLabel
    {
        get
        {
            if (SelectedCalendar == null)
            {
                return L["Calendar"];
            }

            var kind = L[$"Enum:CalendarKind:{SelectedCalendar.Kind}"].Value;
            return string.IsNullOrWhiteSpace(kind)
                ? SelectedCalendar.Name
                : $"{SelectedCalendar.Name} {kind}";
        }
    }

    private bool _hasAppliedInitialCalendarId;

    protected override async Task OnParametersSetAsync()
    {
        if (CalendarOptions.Count == 0)
        {
            await LoadCalendarsAsync();
        }

        await TryApplyInitialCalendarIdAsync();
        SyncSelectedCalendar();
    }

    public virtual async Task RefreshAsync()
    {
        IsLoaded = false;
        _hasAppliedInitialCalendarId = false;
        CalendarOptions.Clear();
        await LoadCalendarsAsync();
    }

    protected virtual Task ToggleAsync()
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        IsOpen = !IsOpen;
        return Task.CompletedTask;
    }

    protected virtual async Task SelectAsync(CalendarLookupDto calendar)
    {
        SelectedCalendarId = calendar.Id;
        SelectedCalendar = calendar;
        IsOpen = false;
        await SelectedCalendarIdChanged.InvokeAsync(calendar.Id);
        await SelectedCalendarChanged.InvokeAsync(calendar);
    }

    protected virtual string GetSwatchStyle(CalendarLookupDto? calendar)
    {
        var color = string.IsNullOrWhiteSpace(calendar?.Color)
            ? CalendarConsts.GetDefaultColor(calendar?.Kind ?? CalendarKind.Personal)
            : calendar!.Color;

        return $"--pro-calendar-color:{color};background:{color};box-shadow:0 0 0 4px color-mix(in srgb, {color} 16%, transparent);";
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
            var visibleCalendars = await CalendarCatalog.GetVisibleCalendarsAsync();
            CalendarOptions.AddRange(visibleCalendars);
        }

        var selected = Calendar.Blazor.Public.Components.CalendarSelectionHelper.ResolveDefaultSelection(
            CalendarOptions,
            SelectedCalendarId,
            InitialCalendarId);

        if (selected != null && SelectedCalendarId == Guid.Empty)
        {
            SelectedCalendarId = selected.Id;
            SelectedCalendar = selected;
            await SelectedCalendarIdChanged.InvokeAsync(selected.Id);
            await SelectedCalendarChanged.InvokeAsync(selected);
        }
        else
        {
            SyncSelectedCalendar();
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
            SelectedCalendar = initial;
            await SelectedCalendarIdChanged.InvokeAsync(initial.Id);
            await SelectedCalendarChanged.InvokeAsync(initial);
        }

        _hasAppliedInitialCalendarId = true;
    }

    private void SyncSelectedCalendar()
    {
        SelectedCalendar = CalendarOptions.FirstOrDefault(x => x.Id == SelectedCalendarId)
                           ?? CalendarOptions.FirstOrDefault();
    }
}

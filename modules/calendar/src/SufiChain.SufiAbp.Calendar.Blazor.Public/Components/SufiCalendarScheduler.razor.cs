using Microsoft.AspNetCore.Components;
using SufiChain.SufiAbp.Calendar.Availability;
using SufiChain.SufiAbp.Calendar.Calendars;
using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public.Components;

public partial class SufiCalendarScheduler : CalendarPublicComponentBase
{
    [Inject]
    protected IAvailabilityCalendarAppService AvailabilityCalendarAppService { get; set; } = default!;

    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public bool Inline { get; set; }

    [Parameter]
    public string? Title { get; set; }

    [Parameter]
    public bool AllowCalendarSelection { get; set; } = true;

    [Parameter]
    public bool AllowEventEditing { get; set; } = true;

    [Parameter]
    public Guid? InitialCalendarId { get; set; }

    [Parameter]
    public DateTime? InitialDate { get; set; }

    [Parameter]
    public IReadOnlyList<CalendarLookupDto>? Calendars { get; set; }

    [Parameter]
    public EventCallback<CalendarEventDto> EventSaved { get; set; }

    protected List<CalendarLookupDto> CalendarOptions { get; set; } = new();
    protected Guid SelectedCalendarId { get; set; }
    protected string? SelectedCalendarKey { get; set; }
    protected string SelectedTimeZoneId { get; set; } = TimeZoneInfo.Local.Id;
    protected SufiCalendarViewMode View { get; set; } = SufiCalendarViewMode.Month;
    protected DateTime Date { get; set; } = DateTime.Today;
    protected bool IsEventEditorOpen { get; set; }
    protected Guid? EditingEventId { get; set; }
    protected DateTime? InitialEventStartUtc { get; set; }
    protected DateTime? InitialEventEndUtc { get; set; }
    protected int RefreshToken { get; set; }
    protected bool HasFocusedInitialDate { get; set; }
    protected IReadOnlyList<Guid> SelectedCalendarIds => SelectedCalendarId == Guid.Empty ? Array.Empty<Guid>() : new[] { SelectedCalendarId };
    protected string CalendarViewKey => $"{SelectedCalendarId:N}-{View}-{Date:yyyyMMdd}-{RefreshToken}";
    protected string CalendarSelectKey => $"{SelectedCalendarKey}-{CalendarOptions.Count}";

    protected override async Task OnParametersSetAsync()
    {
        if (!Open && !Inline)
        {
            return;
        }

        if (InitialDate.HasValue && !HasFocusedInitialDate)
        {
            Date = InitialDate.Value.Date;
            HasFocusedInitialDate = true;
        }

        if (CalendarOptions.Count == 0)
        {
            await LoadCalendarsAsync();
        }
    }

    protected virtual async Task SetOpenAsync(bool value)
    {
        Open = value;
        await OpenChanged.InvokeAsync(value);
    }

    protected virtual async Task CloseAsync()
    {
        await SetOpenAsync(false);
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
                OwnerType = personalCalendar.OwnerType,
                OwnerId = personalCalendar.OwnerId,
                IsDefault = personalCalendar.IsDefault
            });

            var visibleCalendars = await AvailabilityCalendarAppService.GetMyVisibleCalendarsAsync();
            CalendarOptions.AddRange(visibleCalendars.Items.Where(x => x.Id != personalCalendar.Id));
        }

        var selected = InitialCalendarId.HasValue
            ? CalendarOptions.FirstOrDefault(x => x.Id == InitialCalendarId.Value)
            : CalendarOptions.FirstOrDefault(x => x.IsDefault) ?? CalendarOptions.FirstOrDefault();

        if (selected != null)
        {
            SetSelectedCalendar(selected);
            StateHasChanged();
            await FocusNearestEventAsync(selected.Id);
        }
    }

    protected virtual async Task OnSelectedCalendarChangedAsync(string? value)
    {
        var selected = CalendarOptions.FirstOrDefault(x => GetCalendarKey(x) == value);
        if (selected != null)
        {
            SetSelectedCalendar(selected);
            await FocusNearestEventAsync(selected.Id);
        }
    }

    protected virtual Task OnViewChangedAsync(SufiCalendarViewMode value)
    {
        View = value;
        return Task.CompletedTask;
    }

    protected virtual Task OnDateChangedAsync(DateTime value)
    {
        Date = value;
        return Task.CompletedTask;
    }

    protected virtual Task OpenCreateEventAsync()
    {
        if (!AllowEventEditing)
        {
            return Task.CompletedTask;
        }

        EditingEventId = null;
        InitialEventStartUtc = DateTime.UtcNow;
        InitialEventEndUtc = InitialEventStartUtc.Value.AddHours(1);
        IsEventEditorOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task OpenCreateEventFromSlotAsync(SufiCalendarSlotSelectArgs args)
    {
        if (!AllowEventEditing)
        {
            return Task.CompletedTask;
        }

        EditingEventId = null;
        InitialEventStartUtc = args.StartUtc;
        InitialEventEndUtc = args.EndUtc;
        IsEventEditorOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task OpenEditEventAsync(EventOccurrenceDto occurrence)
    {
        if (!AllowEventEditing)
        {
            return Task.CompletedTask;
        }

        EditingEventId = occurrence.EventId;
        InitialEventStartUtc = null;
        InitialEventEndUtc = null;
        IsEventEditorOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task OnEventEditorOpenChangedAsync(bool value)
    {
        IsEventEditorOpen = value;
        return Task.CompletedTask;
    }

    protected virtual async Task OnEventSavedAsync(CalendarEventDto calendarEvent)
    {
        await EventSaved.InvokeAsync(calendarEvent);
        Date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(calendarEvent.StartUtc, DateTimeKind.Utc), ResolveSelectedTimeZone()).Date;
        RefreshToken++;
    }

    protected virtual Task OnEventDeletedAsync()
    {
        RefreshToken++;
        return Task.CompletedTask;
    }

    protected virtual async Task RefreshAsync()
    {
        CalendarOptions.Clear();
        await LoadCalendarsAsync();
    }

    protected virtual string GetCalendarOptionText(CalendarLookupDto calendar)
    {
        return $"{calendar.Name} · {L[$"Enum:CalendarKind:{calendar.Kind}"]}";
    }

    protected virtual string GetCalendarKey(CalendarLookupDto calendar)
    {
        return GetCalendarOptionText(calendar);
    }

    protected virtual async Task FocusNearestEventAsync(Guid calendarId)
    {
        if (InitialDate.HasValue || HasFocusedInitialDate || calendarId == Guid.Empty)
        {
            return;
        }

        var result = await CalendarEventAppService.GetListAsync(new GetEventListInput
        {
            CalendarId = calendarId,
            FromUtc = DateTime.UtcNow.AddYears(-10),
            ToUtc = DateTime.UtcNow.AddYears(10),
            MaxResultCount = 100
        });

        var firstEvent = result.Items
            .OrderBy(x => Math.Abs((x.StartUtc - DateTime.UtcNow).Ticks))
            .FirstOrDefault();
        if (firstEvent != null)
        {
            Date = DateTime.SpecifyKind(firstEvent.StartUtc, DateTimeKind.Utc);
            HasFocusedInitialDate = true;
            RefreshToken++;
        }
    }

    private void SetSelectedCalendar(CalendarLookupDto calendar)
    {
        SelectedCalendarId = calendar.Id;
        SelectedCalendarKey = GetCalendarKey(calendar);
        SelectedTimeZoneId = string.IsNullOrWhiteSpace(calendar.TimeZoneId) ? TimeZoneInfo.Local.Id : calendar.TimeZoneId;
    }

    private TimeZoneInfo ResolveSelectedTimeZone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(SelectedTimeZoneId);
    }
}

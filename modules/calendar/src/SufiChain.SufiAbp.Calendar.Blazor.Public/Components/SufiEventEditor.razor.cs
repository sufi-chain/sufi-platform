using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using SufiChain.SufiAbp.Calendar.Events;

namespace SufiChain.SufiAbp.Calendar.Blazor.Public.Components;

public partial class SufiEventEditor : CalendarPublicComponentBase
{
    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Parameter]
    public bool Open { get; set; }

    [Parameter]
    public EventCallback<bool> OpenChanged { get; set; }

    [Parameter]
    public Guid? EventId { get; set; }

    [Parameter]
    public Guid CalendarId { get; set; }

    [Parameter]
    public DateTime? InitialStartUtc { get; set; }

    [Parameter]
    public DateTime? InitialEndUtc { get; set; }

    [Parameter]
    public string TimeZoneId { get; set; } = TimeZoneInfo.Local.Id;

    [Parameter]
    public EventCallback<CalendarEventDto> Saved { get; set; }

    private CreateUpdateCalendarEventDto _model = new();
    private SbDateRange? _dateRange;
    private string _startTimeText = "09:00";
    private string _endTimeText = "10:00";
    private Guid? _loadedEventId;

    protected string DialogTitle => EventId.HasValue ? L["EditEvent"] : L["CreateEvent"];

    protected override async Task OnParametersSetAsync()
    {
        if (Open && EventId.HasValue && _loadedEventId != EventId)
        {
            var existing = await CalendarEventAppService.GetAsync(EventId.Value);
            _model = new CreateUpdateCalendarEventDto
            {
                CalendarId = existing.CalendarId,
                Title = existing.Title,
                StartUtc = existing.StartUtc,
                EndUtc = existing.EndUtc,
                IsAllDay = existing.IsAllDay,
                TimeZoneId = existing.TimeZoneId,
                Location = existing.Location,
                Description = existing.Description,
                Color = existing.Color,
                Status = existing.Status,
                AvailabilityCalendarId = existing.AvailabilityCalendarId,
                SourceType = existing.SourceType,
                SourceId = existing.SourceId,
                RecurrenceRule = existing.RecurrenceRule,
                ExtraProperties = existing.ExtraProperties
            };
            SyncDateFields();
            _loadedEventId = EventId;
        }
        else if (Open && !EventId.HasValue && _loadedEventId != null)
        {
            ResetModel();
        }
        else if (Open && _model.CalendarId == Guid.Empty)
        {
            ResetModel();
        }
    }

    protected virtual async Task SetOpenAsync(bool value)
    {
        Open = value;
        await OpenChanged.InvokeAsync(value);
    }

    protected virtual async Task HideAsync()
    {
        await SetOpenAsync(false);
    }

    protected virtual void OnDateRangeChanged(SbDateRange? value)
    {
        _dateRange = value;
    }

    protected virtual async Task SaveAsync()
    {
        if (!TryApplyDateTimeFields())
        {
            await Message.ErrorAsync(L["InvalidTimeRange"]);
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            var saved = EventId.HasValue
                ? await CalendarEventAppService.UpdateAsync(EventId.Value, _model)
                : await CalendarEventAppService.CreateAsync(_model);
            await Saved.InvokeAsync(saved);
            await Message.SuccessAsync(L["SavedSuccessfully"]);
            await HideAsync();
        }, LoadingKeys.Save);
    }

    private void ResetModel()
    {
        var startUtc = InitialStartUtc ?? DateTime.UtcNow;
        var endUtc = InitialEndUtc ?? startUtc.AddHours(1);
        _model = new CreateUpdateCalendarEventDto
        {
            CalendarId = CalendarId,
            Title = string.Empty,
            StartUtc = startUtc,
            EndUtc = endUtc,
            TimeZoneId = TimeZoneId,
            Status = EventStatus.Confirmed
        };
        _loadedEventId = null;
        SyncDateFields();
    }

    private void SyncDateFields()
    {
        var timeZone = ResolveTimeZone();
        var startLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(_model.StartUtc, DateTimeKind.Utc), timeZone);
        var endLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(_model.EndUtc, DateTimeKind.Utc), timeZone);
        _dateRange = new SbDateRange(DateOnly.FromDateTime(startLocal), DateOnly.FromDateTime(endLocal));
        _startTimeText = startLocal.ToString("HH:mm");
        _endTimeText = endLocal.ToString("HH:mm");
    }

    private bool TryApplyDateTimeFields()
    {
        if (_dateRange?.Start is null || _dateRange.End is null ||
            !TimeSpan.TryParse(_startTimeText, out var startTime) ||
            !TimeSpan.TryParse(_endTimeText, out var endTime))
        {
            return false;
        }

        var startLocal = _dateRange.Start.Value.ToDateTime(TimeOnly.MinValue).Add(startTime);
        var endLocal = _dateRange.End.Value.ToDateTime(TimeOnly.MinValue).Add(endTime);
        if (endLocal <= startLocal)
        {
            return false;
        }

        var timeZone = ResolveTimeZone();
        _model.StartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), timeZone);
        _model.EndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), timeZone);
        _model.CalendarId = CalendarId == Guid.Empty ? _model.CalendarId : CalendarId;
        _model.TimeZoneId = string.IsNullOrWhiteSpace(_model.TimeZoneId) ? TimeZoneId : _model.TimeZoneId;
        return true;
    }

    private TimeZoneInfo ResolveTimeZone()
    {
        return TimeZoneInfo.FindSystemTimeZoneById(string.IsNullOrWhiteSpace(_model.TimeZoneId) ? TimeZoneId : _model.TimeZoneId);
    }

    private static class LoadingKeys
    {
        public const string Save = "save-event";
    }
}

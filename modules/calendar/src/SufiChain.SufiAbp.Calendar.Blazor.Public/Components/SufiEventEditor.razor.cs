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

    [Parameter]
    public EventCallback Deleted { get; set; }

    private CreateUpdateCalendarEventDto _model = new();
    private SbDateRange? _dateRange;
    private string _startTimeText = "09:00";
    private string _endTimeText = "10:00";
    private Guid? _loadedEventId;
    private int _activeTab;
    private RecurrenceFrequency _recurrenceFrequency;
    private int _recurrenceInterval = 1;
    private int _recurrenceCount;
    private List<EventAttendeeDto> _attendees = new();
    private List<EventReminderDto> _reminders = new();
    private string _newAttendeeDisplayName = string.Empty;
    private string? _newAttendeeEmail;
    private AttendeeRole _newAttendeeRole = AttendeeRole.Required;
    private string _newReminderOffsetText = "-00:15:00";
    private ReminderChannel _newReminderChannel = ReminderChannel.Email;

    protected string DialogTitle => EventId.HasValue ? L["EditEvent"] : L["CreateEvent"];
    protected IReadOnlyList<TimeZoneInfo> TimeZoneOptions { get; } = TimeZoneInfo.GetSystemTimeZones();
    protected IReadOnlyList<EventStatus> EventStatusOptions { get; } = Enum.GetValues<EventStatus>();
    protected IReadOnlyList<RecurrenceFrequency> RecurrenceFrequencyOptions { get; } = Enum.GetValues<RecurrenceFrequency>();
    protected IReadOnlyList<AttendeeRole> AttendeeRoleOptions { get; } = Enum.GetValues<AttendeeRole>();
    protected IReadOnlyList<ReminderChannel> ReminderChannelOptions { get; } = Enum.GetValues<ReminderChannel>();

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
            _attendees = existing.Attendees.ToList();
            _reminders = existing.Reminders.ToList();
            SyncDateFields();
            SyncRecurrenceFields();
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
            EventId = saved.Id;
            _attendees = saved.Attendees.ToList();
            _reminders = saved.Reminders.ToList();
            await Message.SuccessAsync(L["SavedSuccessfully"]);
            await HideAsync();
        }, LoadingKeys.Save);
    }

    protected virtual async Task DeleteAsync()
    {
        if (!EventId.HasValue)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await CalendarEventAppService.DeleteAsync(EventId.Value);
            await Deleted.InvokeAsync();
            await Message.SuccessAsync(L["DeletedSuccessfully"]);
            await HideAsync();
        }, LoadingKeys.Delete);
    }

    protected virtual Task OnRecurrenceFrequencyChanged(RecurrenceFrequency frequency)
    {
        _recurrenceFrequency = frequency;
        ApplyRecurrenceRule();
        return Task.CompletedTask;
    }

    protected virtual Task OnRecurrenceIntervalChanged(int interval)
    {
        _recurrenceInterval = interval <= 0 ? 1 : interval;
        ApplyRecurrenceRule();
        return Task.CompletedTask;
    }

    protected virtual Task OnRecurrenceCountChanged(int count)
    {
        _recurrenceCount = count < 0 ? 0 : count;
        ApplyRecurrenceRule();
        return Task.CompletedTask;
    }

    protected virtual async Task AddAttendeeAsync()
    {
        if (!EventId.HasValue || string.IsNullOrWhiteSpace(_newAttendeeDisplayName))
        {
            return;
        }

        var updated = await CalendarEventAppService.AddAttendeeAsync(EventId.Value, new CreateEventAttendeeDto
        {
            DisplayName = _newAttendeeDisplayName,
            Email = string.IsNullOrWhiteSpace(_newAttendeeEmail) ? null : _newAttendeeEmail,
            Role = _newAttendeeRole
        });
        _attendees = updated.Attendees.ToList();
        _newAttendeeDisplayName = string.Empty;
        _newAttendeeEmail = null;
    }

    protected virtual async Task RemoveAttendeeAsync(EventAttendeeDto attendee)
    {
        if (!EventId.HasValue)
        {
            return;
        }

        var updated = await CalendarEventAppService.RemoveAttendeeAsync(EventId.Value, attendee.Id);
        _attendees = updated.Attendees.ToList();
    }

    protected virtual async Task AddReminderAsync()
    {
        if (!EventId.HasValue || !TimeSpan.TryParse(_newReminderOffsetText, out var offset))
        {
            return;
        }

        var updated = await CalendarEventAppService.AddReminderAsync(EventId.Value, new CreateEventReminderDto
        {
            Offset = offset,
            Channel = _newReminderChannel
        });
        _reminders = updated.Reminders.ToList();
    }

    protected virtual async Task RemoveReminderAsync(EventReminderDto reminder)
    {
        if (!EventId.HasValue)
        {
            return;
        }

        var updated = await CalendarEventAppService.RemoveReminderAsync(EventId.Value, reminder.Id);
        _reminders = updated.Reminders.ToList();
    }

    protected virtual string GetEventStatusText(EventStatus status)
    {
        return L[$"Enum:EventStatus:{status}"];
    }

    protected virtual string GetRecurrenceFrequencyText(RecurrenceFrequency frequency)
    {
        return L[$"Enum:RecurrenceFrequency:{frequency}"];
    }

    protected virtual string GetAttendeeRoleText(AttendeeRole role)
    {
        return L[$"Enum:AttendeeRole:{role}"];
    }

    protected virtual string GetRsvpStatusText(RsvpStatus status)
    {
        return L[$"Enum:RsvpStatus:{status}"];
    }

    protected virtual string GetReminderChannelText(ReminderChannel channel)
    {
        return L[$"Enum:ReminderChannel:{channel}"];
    }

    protected virtual string FormatReminderOffset(TimeSpan offset)
    {
        return offset.ToString(@"hh\:mm\:ss");
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
        _attendees = new List<EventAttendeeDto>();
        _reminders = new List<EventReminderDto>();
        _activeTab = 0;
        SyncDateFields();
        SyncRecurrenceFields();
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

    private void SyncRecurrenceFields()
    {
        _recurrenceFrequency = RecurrenceFrequency.None;
        _recurrenceInterval = 1;
        _recurrenceCount = 0;

        if (string.IsNullOrWhiteSpace(_model.RecurrenceRule))
        {
            return;
        }

        var parts = _model.RecurrenceRule
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => x.Split('=', 2))
            .Where(x => x.Length == 2)
            .ToDictionary(x => x[0], x => x[1], StringComparer.OrdinalIgnoreCase);

        if (parts.TryGetValue("FREQ", out var frequency) && Enum.TryParse<RecurrenceFrequency>(frequency, true, out var parsedFrequency))
        {
            _recurrenceFrequency = parsedFrequency;
        }

        if (parts.TryGetValue("INTERVAL", out var intervalText) && int.TryParse(intervalText, out var interval))
        {
            _recurrenceInterval = interval <= 0 ? 1 : interval;
        }

        if (parts.TryGetValue("COUNT", out var countText) && int.TryParse(countText, out var count))
        {
            _recurrenceCount = count < 0 ? 0 : count;
        }
    }

    private void ApplyRecurrenceRule()
    {
        if (_recurrenceFrequency == RecurrenceFrequency.None)
        {
            _model.RecurrenceRule = null;
            return;
        }

        var frequency = _recurrenceFrequency switch
        {
            RecurrenceFrequency.Daily => "DAILY",
            RecurrenceFrequency.Weekly => "WEEKLY",
            RecurrenceFrequency.Monthly => "MONTHLY",
            _ => string.Empty
        };
        var rule = $"FREQ={frequency};INTERVAL={_recurrenceInterval}";
        if (_recurrenceCount > 0)
        {
            rule += $";COUNT={_recurrenceCount}";
        }

        _model.RecurrenceRule = rule;
    }

    private static class LoadingKeys
    {
        public const string Save = "save-event";
        public const string Delete = "delete-event";
    }

    protected enum RecurrenceFrequency
    {
        None = 0,
        Daily = 1,
        Weekly = 2,
        Monthly = 3
    }
}

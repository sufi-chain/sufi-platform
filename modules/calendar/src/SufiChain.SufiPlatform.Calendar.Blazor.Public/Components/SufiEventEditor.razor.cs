using Microsoft.AspNetCore.Components;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using SufiChain.SufiPlatform.Calendar.Blazor.Public;
using SufiChain.SufiPlatform.Calendar.Events;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class SufiEventEditor : CalendarPublicComponentBase
{
    private const int ReminderOffsetMinuteMax = EventConsts.MaxReminderOffsetDays * 24 * 60;

    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

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
    public string? ReturnUrl { get; set; }

    private CreateUpdateCalendarEventDto _model = new();
    private SbDateRange? _dateRange;
    private TimeOnly? _startTime = new TimeOnly(9, 0);
    private TimeOnly? _endTime = new TimeOnly(10, 0);
    private string? _loadKey;
    private bool _ready;
    private bool _loadFailed;
    private int _activeTab;
    private RecurrenceFrequency _recurrenceFrequency;
    private int _recurrenceInterval = 1;
    private int _recurrenceCount;
    private List<EventAttendeeDto> _attendees = new();
    private List<EventReminderDto> _reminders = new();
    private string _newAttendeeDisplayName = string.Empty;
    private string? _newAttendeeEmail;
    private AttendeeRole _newAttendeeRole = AttendeeRole.Required;
    private int _newReminderOffsetMinutes = 15;
    private ReminderChannel _newReminderChannel = ReminderChannel.Email;

    protected IReadOnlyList<TimeZoneInfo> TimeZoneOptions { get; } = TimeZoneInfo.GetSystemTimeZones();
    protected IReadOnlyList<EventStatus> EventStatusOptions { get; } = Enum.GetValues<EventStatus>();
    protected IReadOnlyList<RecurrenceFrequency> RecurrenceFrequencyOptions { get; } = Enum.GetValues<RecurrenceFrequency>();
    protected IReadOnlyList<AttendeeRole> AttendeeRoleOptions { get; } = Enum.GetValues<AttendeeRole>();
    protected IReadOnlyList<ReminderChannel> ReminderChannelOptions { get; } = Enum.GetValues<ReminderChannel>();

    protected override async Task OnParametersSetAsync()
    {
        var key = EventId.HasValue
            ? $"edit:{EventId.Value:N}"
            : $"new:{CalendarId:N}:{InitialStartUtc:o}:{InitialEndUtc:o}:{TimeZoneId}";
        if (key == _loadKey)
        {
            return;
        }

        _loadKey = key;
        _ready = false;
        _loadFailed = false;
        try
        {
            if (EventId.HasValue)
            {
                await LoadExistingAsync(EventId.Value);
            }
            else
            {
                ResetModel();
            }

            _ready = true;
        }
        catch (Exception exception)
        {
            _loadFailed = true;
            await HandleErrorAsync(exception);
        }
    }

    protected virtual Task LeaveAsync()
    {
        NavigationManager.NavigateTo(CalendarPageRoutes.ResolveReturnUrl(ReturnUrl, CalendarPageRoutes.CurrentLocation(NavigationManager)));
        return Task.CompletedTask;
    }

    protected virtual void OnDateRangeChanged(SbDateRange? value)
    {
        _dateRange = value;
    }

    protected virtual async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(_model.Title))
        {
            await Message.ErrorAsync(L["TitleRequired"]);
            return;
        }

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
            EventId = saved.Id;
            _attendees = saved.Attendees.ToList();
            _reminders = saved.Reminders.ToList();
            await Message.SuccessAsync(L["SavedSuccessfully"]);
            await LeaveAsync();
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
            await Message.SuccessAsync(L["DeletedSuccessfully"]);
            await LeaveAsync();
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
        if (!EventId.HasValue)
        {
            return;
        }

        var minutes = Math.Clamp(_newReminderOffsetMinutes, 0, ReminderOffsetMinuteMax);
        var updated = await CalendarEventAppService.AddReminderAsync(EventId.Value, new CreateEventReminderDto
        {
            Offset = TimeSpan.FromMinutes(-minutes),
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
        var minutes = (int)Math.Abs(offset.TotalMinutes);
        return minutes.ToString(System.Globalization.CultureInfo.CurrentCulture);
    }

    private async Task LoadExistingAsync(Guid eventId)
    {
        var existing = await CalendarEventAppService.GetAsync(eventId);
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
        _startTime = TimeOnly.FromDateTime(startLocal);
        _endTime = TimeOnly.FromDateTime(endLocal);
    }

    private bool TryApplyDateTimeFields()
    {
        if (_dateRange?.Start is null || _dateRange.End is null || _startTime is null || _endTime is null)
        {
            return false;
        }

        var startLocal = _dateRange.Start.Value.ToDateTime(_startTime.Value);
        var endLocal = _dateRange.End.Value.ToDateTime(_endTime.Value);
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

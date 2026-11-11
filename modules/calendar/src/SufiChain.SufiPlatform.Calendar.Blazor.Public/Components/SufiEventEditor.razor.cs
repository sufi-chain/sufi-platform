using System.Runtime.ExceptionServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Logging;
using SufiChain.SufiBlazor.Utilities.DateUtils;
using SufiChain.SufiPlatform.Calendar.Blazor.Public;
using SufiChain.SufiPlatform.Calendar.Events;
using Volo.Abp.Authorization;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Calendar.Blazor.Public.Components;

public partial class SufiEventEditor : CalendarPublicComponentBase
{
    private const int ReminderOffsetMinuteMax = EventConsts.MaxReminderOffsetDays * 24 * 60;

    [Inject]
    protected ICalendarEventAppService CalendarEventAppService { get; set; } = default!;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    protected ISettingProvider SettingProvider { get; set; } = default!;

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
    private string _configuredTimeZoneId = string.Empty;
    private bool _configuredTimeZoneLoaded;
    private int _editorFaults;
    private ErrorBoundary? _editorErrorBoundary;

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
        await EnsureConfiguredTimeZoneAsync();
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

    protected virtual async Task RetryLoadAsync()
    {
        _loadKey = null;
        await OnParametersSetAsync();
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

        if (_recurrenceFrequency != RecurrenceFrequency.None && _recurrenceInterval < 1)
        {
            await Message.ErrorAsync(L["InvalidRecurrenceInterval"]);
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
        _recurrenceInterval = interval;
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

        if (_newReminderOffsetMinutes < 0 || _newReminderOffsetMinutes > ReminderOffsetMinuteMax)
        {
            await Message.ErrorAsync(L["InvalidReminderOffset"]);
            return;
        }

        try
        {
            var updated = await CalendarEventAppService.AddReminderAsync(EventId.Value, new CreateEventReminderDto
            {
                Offset = TimeSpan.FromMinutes(-_newReminderOffsetMinutes),
                Channel = _newReminderChannel
            });
            _reminders = updated.Reminders.ToList();
        }
        catch (Exception exception)
        {
            if (IsFrameworkEditorFault(exception))
            {
                throw;
            }

            Logger.LogWarning(exception, "Could not add a calendar reminder.");
            await Notify.ErrorAsync(L["CalendarActionFailed"]);
        }
    }

    /// <summary>
    /// Authorization, navigation, and cancellation must reach the framework. A toast would swallow a redirect or a disposed circuit.
    /// </summary>
    protected static void RethrowUnhandledEditorFault(Exception exception)
    {
        if (IsFrameworkEditorFault(exception))
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }

    private static bool IsFrameworkEditorFault(Exception exception)
    {
        return exception is AbpAuthorizationException or NavigationException or OperationCanceledException;
    }

    protected virtual Task RecoverEditorAsync()
    {
        _editorFaults++;
        if (_editorFaults > 1)
        {
            return Task.CompletedTask;
        }

        _editorErrorBoundary?.Recover();
        return Task.CompletedTask;
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

        ApplyTimeZoneSelection();
        var timeZone = ResolveTimeZone();
        try
        {
            _model.StartUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(startLocal, DateTimeKind.Unspecified), timeZone);
            _model.EndUtc = TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(endLocal, DateTimeKind.Unspecified), timeZone);
        }
        catch (ArgumentException)
        {
            return false;
        }
        _model.CalendarId = CalendarId == Guid.Empty ? _model.CalendarId : CalendarId;
        return true;
    }

    private void ApplyTimeZoneSelection()
    {
        _model.TimeZoneId = CalendarTiming.ResolveId(KnownZone(_model.TimeZoneId, TimeZoneId), _configuredTimeZoneId);
    }

    private static string? KnownZone(string? selected, string? fromRequest)
    {
        if (CalendarTimeZoneResolver.TryFind(selected) != null)
        {
            return selected;
        }

        if (CalendarTimeZoneResolver.TryFind(fromRequest) != null)
        {
            return fromRequest;
        }

        return null;
    }

    private async Task EnsureConfiguredTimeZoneAsync()
    {
        if (_configuredTimeZoneLoaded)
        {
            return;
        }

        _configuredTimeZoneLoaded = true;
        try
        {
            _configuredTimeZoneId = await SettingProvider.GetOrNullAsync(CalendarTiming.SettingName) ?? string.Empty;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "Could not read the ABP timing time zone.");
            _configuredTimeZoneId = string.Empty;
        }
    }

    private TimeZoneInfo ResolveTimeZone()
    {
        var id = CalendarTiming.ResolveId(KnownZone(_model.TimeZoneId, TimeZoneId), _configuredTimeZoneId);
        return CalendarTimeZoneResolver.Resolve(id, "UTC");
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
            _recurrenceInterval = interval;
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

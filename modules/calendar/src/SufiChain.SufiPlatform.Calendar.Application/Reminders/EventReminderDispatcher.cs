using System.Globalization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Email;
using SufiChain.SufiPlatform.SufiCom.Notifications;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

public class EventReminderDispatcher : IEventReminderDispatcher, ITransientDependency
{
    public const string InAppCategory = "Calendar";

    public const string InAppSource = "Calendar.Reminder";

    private readonly ICalendarEventRepository _eventRepository;
    private readonly IEmailSender _emailSender;
    private readonly INotificationPublisher _notifications;
    private readonly ISettingProvider _settingProvider;
    private readonly IStringLocalizer<CalendarResource> _localizer;
    private readonly ILogger<EventReminderDispatcher> _logger;

    public EventReminderDispatcher(
        ICalendarEventRepository eventRepository,
        IEmailSender emailSender,
        INotificationPublisher notifications,
        ISettingProvider settingProvider,
        IStringLocalizer<CalendarResource> localizer,
        ILogger<EventReminderDispatcher> logger)
    {
        _eventRepository = eventRepository;
        _emailSender = emailSender;
        _notifications = notifications;
        _settingProvider = settingProvider;
        _localizer = localizer;
        _logger = logger;
    }

    public virtual async Task<int> DispatchDueAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var now = DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc);
        var catchUpMinutes = CalendarReminderSettings.ResolveCatchUpMinutes(
            await _settingProvider.GetOrNullAsync(CalendarReminderSettings.CatchUpMinutes));
        var dueItems = await _eventRepository.GetDueRemindersAsync(now, TimeSpan.FromMinutes(catchUpMinutes), cancellationToken);
        var dispatchedCount = 0;

        foreach (var item in dueItems)
        {
            if (item.Skip)
            {
                var reason = item.Occurrence.EndUtc <= now ? "ended" : "past-catch-up";
                item.Event.MarkReminderSkipped(item.Reminder, now, reason);
                await _eventRepository.UpdateAsync(item.Event, cancellationToken: cancellationToken);
                _logger.LogInformation(
                    "Calendar reminder {ReminderId} for event {EventId} skipped ({Reason}).",
                    item.Reminder.Id,
                    item.Event.Id,
                    reason);
                continue;
            }

            if (!await TryDeliverAsync(item, cancellationToken))
            {
                continue;
            }

            item.Reminder.MarkSent(now);
            await _eventRepository.UpdateAsync(item.Event, cancellationToken: cancellationToken);
            dispatchedCount++;
        }

        return dispatchedCount;
    }

    protected virtual async Task<bool> TryDeliverAsync(EventReminderDispatchItem item, CancellationToken cancellationToken)
    {
        if (item.Reminder.Channel == ReminderChannel.InApp)
        {
            var userId = item.Attendee?.UserId;
            if (userId is null)
            {
                _logger.LogWarning(
                    "Calendar reminder {ReminderId} for event {EventId} has no user to notify.",
                    item.Reminder.Id,
                    item.Event.Id);
                return false;
            }

            var title = _localizer["Reminder:InAppTitle", item.Event.Title].Value;
            var body = _localizer["Reminder:InAppBody", item.Event.Title, FormatStart(item)].Value;
            await _notifications.PublishAsync(
                title,
                body,
                new[] { userId.Value },
                InboxNotificationSeverity.Info,
                category: InAppCategory,
                source: InAppSource,
                url: EventUrl(item.Event.Id));
            return true;
        }

        if (item.Reminder.Channel == ReminderChannel.Email && !string.IsNullOrWhiteSpace(item.Attendee?.Email))
        {
            await _emailSender.QueueAsync(
                item.Attendee.Email,
                _localizer["Reminder:EmailSubject", item.Event.Title].Value,
                _localizer["Reminder:EmailBody", item.Event.Title, FormatStart(item), item.Event.Location ?? string.Empty].Value,
                isBodyHtml: false,
                additionalArgs: new AdditionalMessageSendingArgs
                {
                    FromDisplayName = _localizer["EmailSender:DisplayName"]
                });
            return true;
        }

        _logger.LogWarning(
            "Calendar reminder {ReminderId} for event {EventId} has no deliverable address.",
            item.Reminder.Id,
            item.Event.Id);
        return false;
    }

    /// <summary>
    /// Same shape as <c>UserTimeZoneDisplay.FormatWithZone</c>. That helper lives in the Blazor
    /// UI assembly, which the application layer does not reference. The event zone is the
    /// recipient-facing zone stored on the event (Asia/Tehran when the event says so).
    /// </summary>
    public static string FormatStart(EventReminderDispatchItem item)
    {
        var zone = CalendarTimeZoneResolver.Resolve(item.Event.TimeZoneId, "Asia/Tehran");
        var utc = item.Occurrence.StartUtc.Kind switch
        {
            DateTimeKind.Utc => item.Occurrence.StartUtc,
            DateTimeKind.Local => item.Occurrence.StartUtc.ToUniversalTime(),
            _ => DateTime.SpecifyKind(item.Occurrence.StartUtc, DateTimeKind.Utc)
        };
        var local = TimeZoneInfo.ConvertTimeFromUtc(utc, zone);
        return string.Format(CultureInfo.InvariantCulture, "{0:yyyy-MM-dd HH:mm} ({1})", local, zone.Id);
    }

    public static string EventUrl(Guid eventId) => "/panel/portal/calendar/events/" + eventId.ToString("D");
}

using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Calendar.Events;
using SufiChain.SufiPlatform.Calendar.Localization;
using SufiChain.SufiPlatform.Calendar.Reminders;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Email;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Reminders;

public class EventReminderDispatcher : IEventReminderDispatcher, ITransientDependency
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly IEmailSender _emailSender;
    private readonly IStringLocalizer<CalendarResource> _localizer;

    public EventReminderDispatcher(
        ICalendarEventRepository eventRepository,
        IEmailSender emailSender,
        IStringLocalizer<CalendarResource> localizer)
    {
        _eventRepository = eventRepository;
        _emailSender = emailSender;
        _localizer = localizer;
    }

    public virtual async Task<int> DispatchDueAsync(DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var dueItems = await _eventRepository.GetDueRemindersAsync(DateTime.SpecifyKind(nowUtc, DateTimeKind.Utc), cancellationToken);
        var dispatchedCount = 0;

        foreach (var item in dueItems)
        {
            if (item.Reminder.Channel == ReminderChannel.Email && !string.IsNullOrWhiteSpace(item.Attendee?.Email))
            {
                await _emailSender.QueueAsync(
                    item.Attendee.Email,
                    $"Reminder: {item.Event.Title}",
                    BuildEmailBody(item),
                    isBodyHtml: false,
                    additionalArgs: new AdditionalMessageSendingArgs
                    {
                        FromDisplayName = _localizer["EmailSender:DisplayName"]
                    });
            }

            item.Reminder.MarkSent(nowUtc);
            await _eventRepository.UpdateAsync(item.Event, cancellationToken: cancellationToken);
            dispatchedCount++;
        }

        return dispatchedCount;
    }

    private static string BuildEmailBody(EventReminderDispatchItem item)
    {
        return $"{item.Event.Title}\nStarts at: {item.Occurrence.StartUtc:u}\nLocation: {item.Event.Location}";
    }
}

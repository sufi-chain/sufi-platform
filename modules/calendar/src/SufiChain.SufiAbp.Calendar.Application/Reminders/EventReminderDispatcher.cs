using SufiChain.SufiAbp.Calendar.Events;
using SufiChain.SufiAbp.Calendar.Reminders;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.Communications.Email;

namespace SufiChain.SufiAbp.Calendar.Reminders;

public class EventReminderDispatcher : IEventReminderDispatcher, ITransientDependency
{
    private readonly ICalendarEventRepository _eventRepository;
    private readonly IEmailSender _emailSender;

    public EventReminderDispatcher(
        ICalendarEventRepository eventRepository,
        IEmailSender emailSender)
    {
        _eventRepository = eventRepository;
        _emailSender = emailSender;
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
                    isBodyHtml: false);
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

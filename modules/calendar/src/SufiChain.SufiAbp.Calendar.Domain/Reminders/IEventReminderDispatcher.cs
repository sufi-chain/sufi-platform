namespace SufiChain.SufiAbp.Calendar.Reminders;

public interface IEventReminderDispatcher
{
    Task<int> DispatchDueAsync(DateTime nowUtc, CancellationToken cancellationToken = default);
}

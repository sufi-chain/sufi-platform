namespace SufiChain.SufiAbp.Messaging.BackgroundJobs;

public class BackgroundSmsSendingJobArgs
{
    public string PhoneNumber { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? From { get; set; }
    public MessagePriority Priority { get; set; }
}

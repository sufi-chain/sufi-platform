using SufiChain.SufiAbp.Messaging.VoiceCall;

namespace SufiChain.SufiAbp.Messaging.BackgroundJobs;

public class BackgroundVoiceCallSendingJobArgs
{
    public string PhoneNumber { get; set; } = default!;
    public string Message { get; set; } = default!;
    public string? From { get; set; }
    public VoiceCallOptions? VoiceOptions { get; set; }
    public MessagePriority Priority { get; set; }
}

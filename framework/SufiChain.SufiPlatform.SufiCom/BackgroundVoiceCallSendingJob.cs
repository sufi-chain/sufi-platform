using SufiChain.SufiPlatform.SufiCom.VoiceCall;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.BackgroundJobs;

public class BackgroundVoiceCallSendingJob : AsyncBackgroundJob<BackgroundVoiceCallSendingJobArgs>, ITransientDependency
{
    protected IVoiceCallSender VoiceCallSender { get; }

    public BackgroundVoiceCallSendingJob(IVoiceCallSender voiceCallSender)
    {
        VoiceCallSender = voiceCallSender;
    }

    public override async Task ExecuteAsync(BackgroundVoiceCallSendingJobArgs args)
    {
        await VoiceCallSender.SendAsync(
            phoneNumber: args.PhoneNumber,
            message: args.Message,
            from: args.From,
            voiceOptions: args.VoiceOptions,
            additionalArgs: new AdditionalMessageSendingArgs
            {
                Priority = args.Priority,
                QueueMessage = false // Already queued, send directly
            }
        );
    }
}

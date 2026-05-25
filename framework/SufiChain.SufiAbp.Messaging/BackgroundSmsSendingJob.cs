using SufiChain.SufiAbp.Messaging.Sms;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Messaging.BackgroundJobs;

public class BackgroundSmsSendingJob : AsyncBackgroundJob<BackgroundSmsSendingJobArgs>, ITransientDependency
{
    protected ISmsSender SmsSender { get; }

    public BackgroundSmsSendingJob(ISmsSender smsSender)
    {
        SmsSender = smsSender;
    }

    public override async Task ExecuteAsync(BackgroundSmsSendingJobArgs args)
    {
        await SmsSender.SendAsync(
            phoneNumber: args.PhoneNumber,
            message: args.Message,
            from: args.From,
            additionalArgs: new AdditionalMessageSendingArgs
            {
                Priority = args.Priority,
                QueueMessage = false // Already queued, send directly
            }
        );
    }
}

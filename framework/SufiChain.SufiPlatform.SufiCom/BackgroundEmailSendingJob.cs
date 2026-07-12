using SufiChain.SufiPlatform.SufiCom.Email;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.BackgroundJobs;

public class BackgroundEmailSendingJob : AsyncBackgroundJob<BackgroundEmailSendingJobArgs>, ITransientDependency
{
    protected IEmailSender EmailSender { get; }

    public BackgroundEmailSendingJob(IEmailSender emailSender)
    {
        EmailSender = emailSender;
    }

    public override async Task ExecuteAsync(BackgroundEmailSendingJobArgs args)
    {
        await EmailSender.SendAsync(
            to: args.To,
            subject: args.Subject,
            body: args.Body,
            isBodyHtml: args.IsBodyHtml,
            from: args.From,
            replyTo: args.ReplyTo,
            cc: args.Cc,
            bcc: args.Bcc,
            attachments: args.Attachments?.Select(a => new MessageAttachment(
                a.File,
                a.FileName,
                a.ContentType,
                a.ContentId
            )),
            additionalArgs: new AdditionalMessageSendingArgs
            {
                Priority = args.Priority,
                QueueMessage = false // Already queued, send directly
            }
        );
    }
}

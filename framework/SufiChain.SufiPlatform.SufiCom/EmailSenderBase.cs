using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiPlatform.SufiCom.BackgroundJobs;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Email;

/// <summary>
/// Base class for email sender implementations
/// </summary>
public abstract class EmailSenderBase : IEmailSender
{
    public MessageType MessageType => MessageType.Email;
    
    public ILogger<EmailSenderBase> Logger { get; set; }
    
    protected IEmailSenderConfiguration Configuration { get; }
    
    protected IBackgroundJobManager BackgroundJobManager { get; }

    protected EmailSenderBase(
        IEmailSenderConfiguration configuration,
        IBackgroundJobManager backgroundJobManager)
    {
        Configuration = configuration;
        BackgroundJobManager = backgroundJobManager;
        Logger = NullLogger<EmailSenderBase>.Instance;
    }

    public virtual async Task SendAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        string? from = null,
        string? replyTo = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        IEnumerable<MessageAttachment>? attachments = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        if (additionalArgs?.QueueMessage == true)
        {
            await QueueAsync(to, subject, body, isBodyHtml, from, replyTo, cc, bcc, attachments, additionalArgs);
            return;
        }

        from ??= await Configuration.GetDefaultFromAddressAsync();

        var mail = BuildMailMessage(to, subject, body, isBodyHtml, from, replyTo, cc, bcc, attachments);

        try
        {
            await SendEmailAsync(mail);
        }
        finally
        {
            mail.Dispose();
        }
    }

    public virtual async Task QueueAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        string? from = null,
        string? replyTo = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        IEnumerable<MessageAttachment>? attachments = null,
        AdditionalMessageSendingArgs? additionalArgs = null)
    {
        from ??= await Configuration.GetDefaultFromAddressAsync();

        await BackgroundJobManager.EnqueueAsync(
            new BackgroundEmailSendingJobArgs
            {
                To = to,
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml,
                From = from,
                ReplyTo = replyTo,
                Cc = cc?.ToArray(),
                Bcc = bcc?.ToArray(),
                Attachments = attachments?.Select(a => new BackgroundEmailAttachment
                {
                    File = a.File,
                    FileName = a.FileName,
                    ContentType = a.ContentType,
                    ContentId = a.ContentId
                }).ToArray(),
                Priority = additionalArgs?.Priority ?? MessagePriority.Normal
            }
        );
    }

    protected virtual MailMessage BuildMailMessage(
        string to,
        string subject,
        string body,
        bool isBodyHtml,
        string from,
        string? replyTo,
        IEnumerable<string>? cc,
        IEnumerable<string>? bcc,
        IEnumerable<MessageAttachment>? attachments)
    {
        var mail = new MailMessage
        {
            From = new MailAddress(from),
            Subject = subject,
            Body = body,
            IsBodyHtml = isBodyHtml
        };

        mail.To.Add(to);

        if (!string.IsNullOrEmpty(replyTo))
        {
            mail.ReplyToList.Add(replyTo);
        }

        if (cc != null)
        {
            foreach (var address in cc)
            {
                mail.CC.Add(address);
            }
        }

        if (bcc != null)
        {
            foreach (var address in bcc)
            {
                mail.Bcc.Add(address);
            }
        }

        if (attachments != null)
        {
            foreach (var attachment in attachments)
            {
                var mailAttachment = new Attachment(
                    new System.IO.MemoryStream(attachment.File),
                    attachment.FileName,
                    attachment.ContentType
                );

                if (!string.IsNullOrEmpty(attachment.ContentId))
                {
                    mailAttachment.ContentId = attachment.ContentId;
                }

                mail.Attachments.Add(mailAttachment);
            }
        }

        return mail;
    }

    /// <summary>
    /// Implement this method to send the email using your provider
    /// </summary>
    protected abstract Task SendEmailAsync(MailMessage mail);
}

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Messaging.Email;

/// <summary>
/// Null implementation of email sender (does nothing)
/// </summary>
[Dependency(TryRegister = true)]
public class NullEmailSender : IEmailSender, ISingletonDependency
{
    public MessageType MessageType => MessageType.Email;
    
    public ILogger<NullEmailSender> Logger { get; set; }

    public NullEmailSender()
    {
        Logger = NullLogger<NullEmailSender>.Instance;
    }

    public Task SendAsync(
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
        Logger.LogWarning($"NullEmailSender: Skipping email send to {to} with subject: {subject}");
        return Task.CompletedTask;
    }

    public Task QueueAsync(
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
        Logger.LogWarning($"NullEmailSender: Skipping email queue to {to} with subject: {subject}");
        return Task.CompletedTask;
    }
}

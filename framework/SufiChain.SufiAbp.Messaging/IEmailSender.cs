using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Messaging.Email;

/// <summary>
/// Interface for sending email messages
/// </summary>
public interface IEmailSender : IMessageSender
{
    /// <summary>
    /// Sends an email message
    /// </summary>
    Task SendAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        string? from = null,
        string? replyTo = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        IEnumerable<MessageAttachment>? attachments = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );

    /// <summary>
    /// Queues an email message for background sending
    /// </summary>
    Task QueueAsync(
        string to,
        string subject,
        string body,
        bool isBodyHtml = true,
        string? from = null,
        string? replyTo = null,
        IEnumerable<string>? cc = null,
        IEnumerable<string>? bcc = null,
        IEnumerable<MessageAttachment>? attachments = null,
        AdditionalMessageSendingArgs? additionalArgs = null
    );
}

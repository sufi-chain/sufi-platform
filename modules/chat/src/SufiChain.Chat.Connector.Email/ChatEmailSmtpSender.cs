using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using SufiChain.Chat.Connectors.Email.Settings;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Connectors.Email;

public class ChatEmailSmtpSender : ITransientDependency
{
    public ILogger<ChatEmailSmtpSender> Logger { get; set; } = null!;

    public virtual async Task<string> SendAsync(
        ChatEmailConnectorRuntimeSettings settings,
        string to,
        string subject,
        string body,
        string? replyToAddress = null,
        string? inReplyToMessageId = null,
        string? threadMessageId = null,
        CancellationToken cancellationToken = default)
    {
        if (!settings.IsOutboundConfigured)
        {
            throw new InvalidOperationException("Email connector outbound settings are not configured.");
        }

        var messageId = CreateMessageId(settings.DefaultFromAddress!);

        using var mail = new MailMessage
        {
            From = new MailAddress(settings.DefaultFromAddress!),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        mail.To.Add(to);

        if (!replyToAddress.IsNullOrWhiteSpace())
        {
            mail.ReplyToList.Add(replyToAddress);
        }
        else if (!settings.ReplyToAddress.IsNullOrWhiteSpace())
        {
            mail.ReplyToList.Add(settings.ReplyToAddress);
        }

        mail.Headers.Add("Message-ID", FormatMessageId(messageId));

        if (!inReplyToMessageId.IsNullOrWhiteSpace())
        {
            mail.Headers.Add("In-Reply-To", FormatMessageId(inReplyToMessageId));
        }

        if (!threadMessageId.IsNullOrWhiteSpace())
        {
            mail.Headers.Add("References", FormatMessageId(threadMessageId));
        }

        using var smtpClient = new SmtpClient(settings.SmtpHost!, settings.SmtpPort)
        {
            EnableSsl = settings.SmtpUseSsl
        };

        if (!settings.SmtpUserName.IsNullOrWhiteSpace())
        {
            smtpClient.Credentials = new NetworkCredential(settings.SmtpUserName, settings.SmtpPassword);
        }

        await smtpClient.SendMailAsync(mail, cancellationToken);
        Logger.LogInformation("Chat email connector sent message {MessageId} to {Recipient}", messageId, to);

        return messageId;
    }

    protected virtual string CreateMessageId(string fromAddress)
    {
        var domain = fromAddress.Contains('@')
            ? fromAddress[(fromAddress.LastIndexOf('@') + 1)..]
            : "chat.local";

        return $"{Guid.NewGuid():N}@{domain}";
    }

    protected virtual string FormatMessageId(string messageId)
    {
        var normalized = ChatEmailThreadResolver.NormalizeMessageId(messageId);
        return normalized.StartsWith('<') ? normalized : $"<{normalized}>";
    }
}

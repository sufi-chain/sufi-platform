using SufiChain.SufiAbp.Communications.Smtp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Communications.Email;

/// <summary>
/// Delegates to <see cref="NullEmailSender"/> when SMTP is not configured,
/// otherwise uses <see cref="ISmtpEmailSender"/>.
/// </summary>
public class DynamicEmailSender : IEmailSender, ITransientDependency
{
    protected NullEmailSender NullEmailSender { get; }

    protected ISmtpEmailSender SmtpEmailSender { get; }

    protected ISettingProvider SettingProvider { get; }

    public MessageType MessageType => MessageType.Email;

    public DynamicEmailSender(
        NullEmailSender nullEmailSender,
        ISmtpEmailSender smtpEmailSender,
        ISettingProvider settingProvider)
    {
        NullEmailSender = nullEmailSender;
        SmtpEmailSender = smtpEmailSender;
        SettingProvider = settingProvider;
    }

    public async Task SendAsync(
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
        var sender = await GetSenderAsync();
        await sender.SendAsync(
            to,
            subject,
            body,
            isBodyHtml,
            from,
            replyTo,
            cc,
            bcc,
            attachments,
            additionalArgs);
    }

    public async Task QueueAsync(
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
        var sender = await GetSenderAsync();
        await sender.QueueAsync(
            to,
            subject,
            body,
            isBodyHtml,
            from,
            replyTo,
            cc,
            bcc,
            attachments,
            additionalArgs);
    }

    protected virtual async Task<IEmailSender> GetSenderAsync()
    {
        var smtpHost = await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpHost);
        return string.IsNullOrWhiteSpace(smtpHost)
            ? NullEmailSender
            : SmtpEmailSender;
    }
}

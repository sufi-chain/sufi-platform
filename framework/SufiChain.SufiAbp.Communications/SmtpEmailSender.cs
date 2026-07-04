using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using SufiChain.SufiAbp.Communications.Email;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Microsoft.Extensions.Logging;

namespace SufiChain.SufiAbp.Communications.Smtp;

/// <summary>
/// SMTP email sender implementation
/// </summary>
public class SmtpEmailSender : EmailSenderBase, ISmtpEmailSender, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public SmtpEmailSender(
        IEmailSenderConfiguration configuration,
        IBackgroundJobManager backgroundJobManager,
        ISettingProvider settingProvider)
        : base(configuration, backgroundJobManager)
    {
        SettingProvider = settingProvider;
    }

    protected override async Task SendEmailAsync(MailMessage mail)
    {
        var host = await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpHost);
        if (string.IsNullOrEmpty(host))
        {
            Logger.LogWarning("SMTP Host is not configured. Email will not be sent.");
            return;
        }

        var port = int.Parse(await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpPort) ?? "25");
        var enableSsl = bool.Parse(await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpEnableSsl) ?? "false");
        var useDefaultCredentials = bool.Parse(await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpUseDefaultCredentials) ?? "true");
        var userName = await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpUserName);
        var password = await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpPassword);
        var domain = await SettingProvider.GetOrNullAsync(CommunicationsSettingNames.Email.SmtpDomain);

        using var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = useDefaultCredentials
        };

        if (!useDefaultCredentials && !string.IsNullOrEmpty(userName))
        {
            smtpClient.Credentials = string.IsNullOrEmpty(domain)
                ? new NetworkCredential(userName, password)
                : new NetworkCredential(userName, password, domain);
        }

        await smtpClient.SendMailAsync(mail);
        
        Logger.LogInformation($"Email sent successfully to {mail.To} via SMTP");
    }
}

using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using SufiChain.SufiAbp.Account.Localization;
using SufiChain.SufiAbp.Account.Templates;
using SufiChain.SufiAbp.Communications.Email;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Account;

public class EmailVerificationChannelSender : IVerificationChannelSender, ITransientDependency
{
    public VerificationDeliveryChannel Channel => VerificationDeliveryChannel.Email;

    protected IEmailSender EmailSender { get; }

    protected ITemplateRenderer TemplateRenderer { get; }

    protected IStringLocalizer<SufiAbpAccountResource> Localizer { get; }

    public EmailVerificationChannelSender(
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer,
        IStringLocalizer<SufiAbpAccountResource> localizer)
    {
        EmailSender = emailSender;
        TemplateRenderer = templateRenderer;
        Localizer = localizer;
    }

    public virtual async Task SendAsync(VerificationMessage message)
    {
        var (templateName, subjectKey) = GetTemplateInfo(message.Purpose);

        var body = await TemplateRenderer.RenderAsync(
            templateName,
            new
            {
                link = message.Link,
                code = message.Code,
                userName = message.UserName,
                appName = message.AppName
            });

        await EmailSender.QueueAsync(
            message.Recipient,
            Localizer[subjectKey],
            body,
            isBodyHtml: true);
    }

    protected virtual (string TemplateName, string SubjectKey) GetTemplateInfo(VerificationPurpose purpose)
    {
        return purpose switch
        {
            VerificationPurpose.EmailConfirmation =>
                (AccountTemplates.EmailConfirmation, "EmailConfirmation:Subject"),
            VerificationPurpose.PasswordReset =>
                (AccountTemplates.PasswordReset, "PasswordReset:Subject"),
            _ =>
                (AccountTemplates.VerificationCode, "VerificationCode:Subject")
        };
    }
}

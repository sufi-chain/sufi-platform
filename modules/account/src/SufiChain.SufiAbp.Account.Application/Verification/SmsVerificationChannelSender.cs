using System.Threading.Tasks;
using SufiChain.SufiAbp.Account.Templates;
using SufiChain.SufiAbp.Messaging;
using SufiChain.SufiAbp.Messaging.Sms;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Account;

/// <summary>
/// Sends verification codes via <see cref="ISmsSender"/>.
/// </summary>
public class SmsVerificationChannelSender : IVerificationChannelSender, ITransientDependency
{
    public VerificationDeliveryChannel Channel => VerificationDeliveryChannel.Sms;

    protected ISmsSender SmsSender { get; }

    protected ITemplateRenderer TemplateRenderer { get; }

    public SmsVerificationChannelSender(
        ISmsSender smsSender,
        ITemplateRenderer templateRenderer)
    {
        SmsSender = smsSender;
        TemplateRenderer = templateRenderer;
    }

    public virtual async Task SendAsync(VerificationMessage message)
    {
        var templateName = GetTemplateName(message.Purpose);

        var body = await TemplateRenderer.RenderAsync(
            templateName,
            new
            {
                code = message.Code,
                userName = message.UserName,
                appName = message.AppName
            });

        await SmsSender.QueueAsync(
            message.Recipient,
            body,
            additionalArgs: new AdditionalMessageSendingArgs { QueueMessage = true });
    }

    protected virtual string GetTemplateName(VerificationPurpose purpose)
    {
        return purpose switch
        {
            VerificationPurpose.TwoFactorCode => AccountTemplates.TwoFactorCodeSms,
            VerificationPurpose.OtpLogin => AccountTemplates.OtpCodeSms,
            VerificationPurpose.OtpRegistration => AccountTemplates.OtpCodeSms,
            _ => AccountTemplates.VerificationCodeSms
        };
    }
}

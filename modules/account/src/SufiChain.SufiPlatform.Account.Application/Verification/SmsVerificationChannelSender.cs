using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiPlatform.Account.Templates;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Sms;
using SufiChain.SufiPlatform.TextTemplating;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Sends verification codes via <see cref="ISmsSender.SendOtpAsync"/>, which uses the provider's transactional OTP route.
/// </summary>
public class SmsVerificationChannelSender : IVerificationChannelSender, ITransientDependency
{
    public VerificationDeliveryChannel Channel => VerificationDeliveryChannel.Sms;

    public ILogger<SmsVerificationChannelSender> Logger { get; set; } = NullLogger<SmsVerificationChannelSender>.Instance;

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

        try
        {
            await SmsSender.SendOtpAsync(CreateOtpMessage(message, body));
        }
        catch (Exception ex)
        {
            // Delivery failures are logged and audited, never surfaced: the caller must not learn whether the recipient exists.
            Logger.LogError(
                "Verification SMS for purpose {Purpose} could not be sent. ExceptionType={ExceptionType}",
                message.Purpose,
                ex.GetType().Name);
        }
    }

    public static OtpMessage CreateOtpMessage(VerificationMessage message, string body)
    {
        return new OtpMessage
        {
            Phone = message.Recipient,
            Code = message.Code ?? string.Empty,
            Purpose = ToOtpPurpose(message.Purpose),
            Content = body,
            AppName = message.AppName,
            IdempotencyKey = $"{message.Purpose}:{message.Recipient}:{Guid.NewGuid():N}"
        };
    }

    public static string ToOtpPurpose(VerificationPurpose purpose)
    {
        return purpose switch
        {
            VerificationPurpose.OtpLogin => OtpPurposes.Login,
            VerificationPurpose.OtpRegistration => OtpPurposes.Registration,
            VerificationPurpose.TwoFactorCode => OtpPurposes.TwoFactor,
            VerificationPurpose.PhoneConfirmation => OtpPurposes.PhoneConfirmation,
            _ => OtpPurposes.Verification
        };
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

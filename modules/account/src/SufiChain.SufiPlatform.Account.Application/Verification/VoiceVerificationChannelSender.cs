using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiPlatform.Account.Templates;
using SufiChain.SufiPlatform.SufiCom.VoiceCall;
using SufiChain.SufiPlatform.TextTemplating;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Delivers verification codes via voice call using <see cref="IVoiceCallSender.SendOtpAsync"/>,
/// which uses the provider's transactional OTP route.
/// </summary>
public class VoiceVerificationChannelSender : IVerificationChannelSender, ITransientDependency
{
    public VerificationDeliveryChannel Channel => VerificationDeliveryChannel.Voice;

    public ILogger<VoiceVerificationChannelSender> Logger { get; set; } = NullLogger<VoiceVerificationChannelSender>.Instance;

    protected IVoiceCallSender VoiceCallSender { get; }

    protected ITemplateRenderer TemplateRenderer { get; }

    public VoiceVerificationChannelSender(
        IVoiceCallSender voiceCallSender,
        ITemplateRenderer templateRenderer)
    {
        VoiceCallSender = voiceCallSender;
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
            await VoiceCallSender.SendOtpAsync(SmsVerificationChannelSender.CreateOtpMessage(message, body));
        }
        catch (Exception ex)
        {
            // Delivery failures are logged and audited, never surfaced: the caller must not learn whether the recipient exists.
            Logger.LogError(
                "Verification call for purpose {Purpose} could not be placed. ExceptionType={ExceptionType}",
                message.Purpose,
                ex.GetType().Name);
        }
    }

    protected virtual string GetTemplateName(VerificationPurpose purpose)
    {
        return purpose switch
        {
            VerificationPurpose.TwoFactorCode => AccountTemplates.TwoFactorCodeVoice,
            VerificationPurpose.OtpLogin => AccountTemplates.OtpCodeVoice,
            VerificationPurpose.OtpRegistration => AccountTemplates.OtpCodeVoice,
            _ => AccountTemplates.VerificationCodeVoice
        };
    }
}

using System.Threading.Tasks;
using SufiChain.SufiAbp.Account.Templates;
using SufiChain.SufiAbp.Communications;
using SufiChain.SufiAbp.Communications.VoiceCall;
using SufiChain.SufiAbp.TextTemplating;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Account;

/// <summary>
/// Delivers verification codes via text-to-speech voice call using <see cref="IVoiceCallSender"/>.
/// </summary>
public class VoiceVerificationChannelSender : IVerificationChannelSender, ITransientDependency
{
    public VerificationDeliveryChannel Channel => VerificationDeliveryChannel.Voice;

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

        await VoiceCallSender.QueueAsync(
            message.Recipient,
            body,
            additionalArgs: new AdditionalMessageSendingArgs { QueueMessage = true });
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

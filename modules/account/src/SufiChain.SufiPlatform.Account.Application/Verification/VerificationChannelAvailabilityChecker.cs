using System.Collections.Generic;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Sms;
using SufiChain.SufiPlatform.SufiCom.VoiceCall;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account;

public class VerificationChannelAvailabilityChecker : IVerificationChannelAvailabilityChecker, ITransientDependency
{
    protected ISmsSender SmsSender { get; }

    protected IVoiceCallSender VoiceCallSender { get; }

    protected ISettingProvider SettingProvider { get; }

    public VerificationChannelAvailabilityChecker(
        ISmsSender smsSender,
        IVoiceCallSender voiceCallSender,
        ISettingProvider settingProvider)
    {
        SmsSender = smsSender;
        VoiceCallSender = voiceCallSender;
        SettingProvider = settingProvider;
    }

    public virtual async Task<IReadOnlyList<VerificationDeliveryChannel>> GetAvailableChannelsAsync()
    {
        var channels = new List<VerificationDeliveryChannel>();

        if (await IsEmailAvailableAsync())
        {
            channels.Add(VerificationDeliveryChannel.Email);
        }

        if (SmsSender is not NullSmsSender)
        {
            channels.Add(VerificationDeliveryChannel.Sms);
        }

        if (VoiceCallSender is not NullVoiceCallSender)
        {
            channels.Add(VerificationDeliveryChannel.Voice);
        }

        return channels;
    }

    protected virtual async Task<bool> IsEmailAvailableAsync()
    {
        var smtpHost = await SettingProvider.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost);
        return !string.IsNullOrWhiteSpace(smtpHost);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account;

public class VerificationChannelResolver : IVerificationChannelResolver, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    protected IVerificationChannelAvailabilityChecker AvailabilityChecker { get; }

    public VerificationChannelResolver(
        ISettingProvider settingProvider,
        IVerificationChannelAvailabilityChecker availabilityChecker)
    {
        SettingProvider = settingProvider;
        AvailabilityChecker = availabilityChecker;
    }

    public virtual async Task<VerificationDeliveryChannel> ResolveAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel? preferredChannel = null)
    {
        var availableChannels = await AvailabilityChecker.GetAvailableChannelsAsync();
        if (availableChannels.Count == 0)
        {
            return VerificationDeliveryChannel.Email;
        }

        if (preferredChannel.HasValue && availableChannels.Contains(preferredChannel.Value))
        {
            return preferredChannel.Value;
        }

        var defaultChannelSetting = purpose switch
        {
            VerificationPurpose.OtpLogin or VerificationPurpose.OtpRegistration =>
                IdentitySettingNames.Otp.DefaultChannel,
            VerificationPurpose.TwoFactorCode =>
                IdentitySettingNames.TwoFactor.CodeDeliveryChannel,
            VerificationPurpose.EmailConfirmation or VerificationPurpose.PasswordReset =>
                null,
            _ => null
        };

        if (defaultChannelSetting != null)
        {
            var configured = await SettingProvider.GetOrNullAsync(defaultChannelSetting);
            if (TryParseChannel(configured, out var configuredChannel) &&
                availableChannels.Contains(configuredChannel))
            {
                return configuredChannel;
            }
        }

        return availableChannels.Contains(VerificationDeliveryChannel.Email)
            ? VerificationDeliveryChannel.Email
            : availableChannels.First();
    }

    protected static bool TryParseChannel(string? value, out VerificationDeliveryChannel channel)
    {
        channel = VerificationDeliveryChannel.Email;
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return Enum.TryParse(value, ignoreCase: true, out channel);
    }
}

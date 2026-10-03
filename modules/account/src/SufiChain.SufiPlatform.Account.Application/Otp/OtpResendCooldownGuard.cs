using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account.Otp;

/// <summary>
/// Applies the per-phone resend cooldown to phone channels. Call it before looking up the user so the
/// response is the same whether or not an account exists.
/// </summary>
public class OtpResendCooldownGuard : ITransientDependency
{
    public const string SecondsDataKey = "seconds";

    protected IOtpResendCooldownStore CooldownStore { get; }

    protected ISettingProvider SettingProvider { get; }

    public OtpResendCooldownGuard(
        IOtpResendCooldownStore cooldownStore,
        ISettingProvider settingProvider)
    {
        CooldownStore = cooldownStore;
        SettingProvider = settingProvider;
    }

    /// <returns>Seconds until the next request for this number is accepted; 0 for non-phone channels.</returns>
    public virtual async Task<int> EnsureAllowedAsync(
        VerificationDeliveryChannel channel,
        string identifier,
        CancellationToken cancellationToken = default)
    {
        if (!channel.IsPhoneChannel())
        {
            return 0;
        }

        var options = await OtpResendCooldownOptions.CreateAsync(SettingProvider);
        var result = await CooldownStore.TryAcquireAsync(identifier, options, cancellationToken);
        if (!result.IsAllowed)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.OtpResendCooldown)
                .WithData(SecondsDataKey, result.WaitSeconds);
        }

        return result.WaitSeconds;
    }
}

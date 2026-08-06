using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Normalizes verification identifiers and resolves users by email or phone.
/// </summary>
public static class VerificationIdentifierHelper
{
    private static readonly Regex NonDigitRegex = new(@"[^\d+]", RegexOptions.Compiled);

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    public static string NormalizePhone(string phone)
    {
        var trimmed = phone.Trim();
        if (trimmed.StartsWith('+'))
        {
            return "+" + NonDigitRegex.Replace(trimmed[1..], string.Empty);
        }

        return NonDigitRegex.Replace(trimmed, string.Empty);
    }

    public static async Task<string> ResolveRecipientAsync(
        IdentityUser user,
        IdentityUserManager userManager,
        VerificationDeliveryChannel channel)
    {
        if (channel == VerificationDeliveryChannel.Email)
        {
            var email = await userManager.GetEmailAsync(user);
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new BusinessException(IdentitySecurityErrorCodes.VerificationChannelUnavailable)
                    .WithData("Channel", channel);
            }

            return email;
        }

        var phone = await userManager.GetPhoneNumberAsync(user);
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberRequired);
        }

        return NormalizePhone(phone);
    }

    public static async Task EnsurePhoneReadyForDeliveryAsync(
        IdentityUser user,
        IdentityUserManager userManager,
        ISettingProvider settingProvider,
        VerificationDeliveryChannel channel)
    {
        if (!channel.IsPhoneChannel())
        {
            return;
        }

        var phone = await userManager.GetPhoneNumberAsync(user);
        if (string.IsNullOrWhiteSpace(phone))
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberRequired);
        }

        if (await settingProvider.IsTrueAsync(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber) &&
            !user.PhoneNumberConfirmed)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.PhoneNumberNotConfirmed);
        }
    }

    public static async Task<IdentityUser?> FindUserByIdentifierAsync(
        IdentityUserManager userManager,
        IIdentityUserRepository userRepository,
        VerificationDeliveryChannel channel,
        string identifier)
    {
        if (channel == VerificationDeliveryChannel.Email)
        {
            return await userManager.FindByEmailAsync(NormalizeEmail(identifier));
        }

        return await FindByPhoneNumberAsync(userRepository, NormalizePhone(identifier));
    }

    public static async Task<IdentityUser?> FindByPhoneNumberAsync(
        IIdentityUserRepository userRepository,
        string normalizedPhone)
    {
        var candidates = await userRepository.GetListAsync(
            phoneNumber: normalizedPhone,
            maxResultCount: 10);

        return candidates.FirstOrDefault(user =>
            !string.IsNullOrWhiteSpace(user.PhoneNumber) &&
            string.Equals(NormalizePhone(user.PhoneNumber), normalizedPhone, StringComparison.Ordinal));
    }

    public static string NormalizeIdentifier(VerificationDeliveryChannel channel, string identifier) =>
        channel == VerificationDeliveryChannel.Email
            ? NormalizeEmail(identifier)
            : NormalizePhone(identifier);

    public static string GetTwoFactorTokenProvider(VerificationDeliveryChannel channel) =>
        channel == VerificationDeliveryChannel.Email
            ? TwoFactorProviderNames.Email
            : TwoFactorProviderNames.Phone;
}

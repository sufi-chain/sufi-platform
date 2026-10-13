using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using SufiChain.SufiPlatform.SufiCom.Channels;
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
        var national = IranianMobileNumber.ToNationalNumber(phone);
        if (national != null)
        {
            return "+98" + national;
        }

        var folded = FoldDigits(phone.Trim());
        if (folded.StartsWith('+'))
        {
            return "+" + NonDigitRegex.Replace(folded[1..], string.Empty);
        }

        return NonDigitRegex.Replace(folded, string.Empty);
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

        if (await IdentityPhoneConfirmationRules.IsRequiredForRegistrationAsync(settingProvider) &&
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
        var canonical = NormalizePhone(normalizedPhone);
        var seen = new HashSet<Guid>();

        foreach (var variant in PhoneLookupVariants(canonical))
        {
            var candidates = await userRepository.GetListAsync(
                phoneNumber: variant,
                maxResultCount: 10);

            foreach (var user in candidates)
            {
                if (!seen.Add(user.Id) || string.IsNullOrWhiteSpace(user.PhoneNumber))
                {
                    continue;
                }

                if (string.Equals(NormalizePhone(user.PhoneNumber), canonical, StringComparison.Ordinal))
                {
                    return user;
                }
            }
        }

        return null;
    }

    public static IReadOnlyList<string> PhoneLookupVariants(string phone)
    {
        var national = IranianMobileNumber.ToNationalNumber(phone);
        if (national == null)
        {
            return [NormalizePhone(phone)];
        }

        return
        [
            "+98" + national,
            "0" + national,
            national,
            "0098" + national,
            "98" + national
        ];
    }

    private static string FoldDigits(string value)
    {
        var builder = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (ch >= '\u06F0' && ch <= '\u06F9')
            {
                builder.Append((char)('0' + (ch - '\u06F0')));
            }
            else if (ch >= '\u0660' && ch <= '\u0669')
            {
                builder.Append((char)('0' + (ch - '\u0660')));
            }
            else
            {
                builder.Append(ch);
            }
        }

        return builder.ToString();
    }

    public static string NormalizeIdentifier(VerificationDeliveryChannel channel, string identifier) =>
        channel == VerificationDeliveryChannel.Email
            ? NormalizeEmail(identifier)
            : NormalizePhone(identifier);

    public static string MaskIdentifier(VerificationDeliveryChannel channel, string identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return "***";
        }

        if (channel == VerificationDeliveryChannel.Email || identifier.Contains('@', StringComparison.Ordinal))
        {
            return MaskEmail(identifier.Trim());
        }

        return MaskPhone(NormalizePhone(identifier));
    }

    public static string GetTwoFactorTokenProvider(VerificationDeliveryChannel channel) =>
        channel == VerificationDeliveryChannel.Email
            ? TwoFactorProviderNames.Email
            : TwoFactorProviderNames.Phone;

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 0 || at == email.Length - 1)
        {
            return "***";
        }

        var local = email[..at];
        var domain = email[(at + 1)..];
        var head = local.Length > 0 ? local[0].ToString() : string.Empty;
        return head + "***@" + domain;
    }

    private static string MaskPhone(string phone)
    {
        if (phone.Length <= 6)
        {
            return "***";
        }

        if (phone.Length <= 10)
        {
            return phone[..2] + "***" + phone[^2..];
        }

        return phone[..6] + "***" + phone[^4..];
    }
}

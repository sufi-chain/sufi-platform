using System.ComponentModel.DataAnnotations;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// Normalizes a profile update before identity writes it.
/// Phone length is checked on the normalized value. A formatted number such as
/// "+98 (912) 345-6789" is longer than <see cref="IdentityUserConsts.MaxPhoneNumberLength"/>
/// and was rejected by model validation with HTTP 400.
/// </summary>
public static class ProfileUpdateRules
{
    public static string? NormalizeOptionalText(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }

    public static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = VerificationIdentifierHelper.NormalizePhone(value);
        if (string.IsNullOrEmpty(normalized) || normalized == "+")
        {
            return null;
        }

        return normalized;
    }

    public static bool SamePhone(string? stored, string? submitted)
    {
        return string.Equals(NormalizePhone(stored), NormalizePhone(submitted), StringComparison.Ordinal);
    }

    public static void EnsurePhoneFits(string? normalizedPhone)
    {
        if (normalizedPhone == null || normalizedPhone.Length <= IdentityUserConsts.MaxPhoneNumberLength)
        {
            return;
        }

        throw new AbpValidationException(
            "The phone number is too long.",
            new List<ValidationResult>
            {
                new(
                    $"The field PhoneNumber must be a string with a maximum length of {IdentityUserConsts.MaxPhoneNumberLength}.",
                    new[] { nameof(UpdateProfileDto.PhoneNumber) })
            });
    }
}

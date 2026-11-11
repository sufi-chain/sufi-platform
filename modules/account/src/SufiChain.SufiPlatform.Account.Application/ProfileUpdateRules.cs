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
        if (!TryNormalizePhone(value, out var normalized))
        {
            throw new AbpValidationException(
                "The phone number is not valid.",
                new List<ValidationResult>
                {
                    new(
                        OptionalPhoneAttribute.ErrorMessageTemplate,
                        new[] { nameof(UpdateProfileDto.PhoneNumber) })
                });
        }

        return normalized;
    }

    public static bool SamePhone(string? stored, string? submitted)
    {
        TryNormalizePhone(stored, out var left);
        TryNormalizePhone(submitted, out var right);
        return string.Equals(left, right, StringComparison.Ordinal);
    }

    private static bool TryNormalizePhone(string? value, out string? normalized)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            normalized = null;
            return true;
        }

        if (!OptionalPhoneAttribute.IsValidPhone(value))
        {
            normalized = null;
            return false;
        }

        var candidate = VerificationIdentifierHelper.NormalizePhone(value);
        if (string.IsNullOrEmpty(candidate) || candidate == "+")
        {
            normalized = null;
            return false;
        }

        normalized = candidate;
        return true;
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

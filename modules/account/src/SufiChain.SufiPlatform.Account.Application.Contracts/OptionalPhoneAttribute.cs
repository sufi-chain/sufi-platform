using System.ComponentModel.DataAnnotations;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// A blank phone clears the stored number. Any other value must be a phone number
/// (<see cref="PhoneAttribute"/>), so a parse failure cannot be saved as empty.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class OptionalPhoneAttribute : ValidationAttribute
{
    public const string ErrorMessageTemplate = "The {0} field is not a valid phone number.";

    public OptionalPhoneAttribute()
    {
        ErrorMessage = ErrorMessageTemplate;
    }

    public override bool IsValid(object? value) => IsValidPhone(value);

    public static bool IsValidPhone(object? value)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        return new PhoneAttribute().IsValid(text);
    }
}

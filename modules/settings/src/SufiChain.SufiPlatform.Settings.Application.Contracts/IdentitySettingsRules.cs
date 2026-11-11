using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.Settings;

/// <summary>
/// Bounds for the numeric identity and lockout settings on the security page.
/// Password length matches ABP Identity (6..128).
/// </summary>
public static class IdentitySettingsRules
{
    public static void Validate(IdentitySettingsDto input)
    {
        var results = new List<ValidationResult>();
        RequireRange(results, input.PasswordRequiredLength, 6, 128, nameof(input.PasswordRequiredLength));
        RequireRange(results, input.PasswordRequiredUniqueChars, 1, 128, nameof(input.PasswordRequiredUniqueChars));
        RequireRange(results, input.PasswordMinimumAgeMinutes, 0, 525600, nameof(input.PasswordMinimumAgeMinutes));
        RequireRange(results, input.LockoutMaxFailedAccessAttempts, 1, 100, nameof(input.LockoutMaxFailedAccessAttempts));
        RequireRange(results, input.LockoutDefaultLockoutTimeSpanMinutes, 1, 1440, nameof(input.LockoutDefaultLockoutTimeSpanMinutes));
        RequireRange(results, input.EmailConfirmationTokenLifespanHours, 1, 168, nameof(input.EmailConfirmationTokenLifespanHours));
        RequireRange(results, input.PasswordResetTokenLifespanHours, 1, 168, nameof(input.PasswordResetTokenLifespanHours));
        RequireRange(results, input.OtpTokenLifespanMinutes, 1, 60, nameof(input.OtpTokenLifespanMinutes));
        RequireRange(results, input.OtpLength, 4, 10, nameof(input.OtpLength));
        RequireRange(results, input.OtpMaxAttemptsPerCode, 1, 20, nameof(input.OtpMaxAttemptsPerCode));
        RequireRange(results, input.OtpRateLimitPerIdentifierPerHour, 1, 100, nameof(input.OtpRateLimitPerIdentifierPerHour));
        RequireRange(results, input.OtpResendCooldownSeconds, 0, 86400, nameof(input.OtpResendCooldownSeconds));
        RequireRange(results, input.CaptchaChallengeExpirationMinutes, 1, 60, nameof(input.CaptchaChallengeExpirationMinutes));
        if (results.Count > 0)
        {
            throw new AbpValidationException(results);
        }
    }

    private static void RequireRange(List<ValidationResult> results, int value, int min, int max, string member)
    {
        if (value < min || value > max)
        {
            results.Add(new ValidationResult($"{member} must be between {min} and {max}.", new[] { member }));
        }
    }
}

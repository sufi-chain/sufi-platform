using System.Collections.Generic;
using SufiChain.SufiPlatform.Account;

namespace SufiChain.SufiPlatform.Settings;

public class IdentitySettingsDto
{
    public bool EnableSelfRegistration { get; set; } = true;

    public bool RequireEmailConfirmation { get; set; }

    public bool RequireConfirmedAccount { get; set; }

    public bool RequireConfirmedEmail { get; set; }

    public bool RequireConfirmedPhoneNumber { get; set; }

    public bool RequireUniqueEmail { get; set; } = true;

    public int PasswordRequiredLength { get; set; } = 6;

    public int PasswordRequiredUniqueChars { get; set; } = 1;

    public bool PasswordRequireNonAlphanumeric { get; set; }

    public bool PasswordRequireLowercase { get; set; }

    public bool PasswordRequireUppercase { get; set; }

    public bool PasswordRequireDigit { get; set; }

    public bool PasswordDisallowUsername { get; set; }

    public bool PasswordDisallowEmail { get; set; }

    public int PasswordMinimumAgeMinutes { get; set; }

    public bool LockoutAllowedForNewUsers { get; set; } = true;

    public int LockoutMaxFailedAccessAttempts { get; set; } = 5;

    public int LockoutDefaultLockoutTimeSpanMinutes { get; set; } = 5;

    public int EmailConfirmationTokenLifespanHours { get; set; } = 24;

    public int PasswordResetTokenLifespanHours { get; set; } = 1;

    public int OtpTokenLifespanMinutes { get; set; } = 5;

    public int OtpLength { get; set; } = 6;

    public bool TwoFactorEnabled { get; set; } = true;

    public bool TwoFactorIsRequired { get; set; }

    public bool TwoFactorUsersCanChange { get; set; } = true;

    public bool TwoFactorEnforceForNewUsers { get; set; }

    public bool TwoFactorEnforceForAdministrators { get; set; }

    public bool TwoFactorAllowAuthenticatorApp { get; set; } = true;

    public bool TwoFactorAllowCodeDelivery { get; set; }

    public string TwoFactorCodeDeliveryChannel { get; set; } = "Email";

    public bool TwoFactorAllowEmailChannel { get; set; } = true;

    public bool TwoFactorAllowSmsChannel { get; set; }

    public bool TwoFactorAllowVoiceChannel { get; set; }

    public bool OtpIsEnabled { get; set; }

    public bool OtpAllowRegistration { get; set; }

    public bool OtpAllowLogin { get; set; }

    public string OtpDefaultChannel { get; set; } = "Email";

    public bool OtpAllowEmailChannel { get; set; } = true;

    public bool OtpAllowSmsChannel { get; set; }

    public bool OtpAllowVoiceChannel { get; set; }

    public int OtpMaxAttemptsPerCode { get; set; } = 5;

    public int OtpRateLimitPerIdentifierPerHour { get; set; } = 10;

    public bool CaptchaIsEnabled { get; set; } = true;

    public string CaptchaProvider { get; set; } = "Simple";

    public bool CaptchaRequiredOnRegister { get; set; } = true;

    public bool CaptchaRequiredOnForgotPassword { get; set; } = true;

    public bool CaptchaRequiredOnOtpSend { get; set; } = true;

    public bool CaptchaRequiredOnLogin { get; set; }

    public bool CaptchaRequiredOnEmailConfirmationResend { get; set; } = true;

    public int CaptchaChallengeExpirationMinutes { get; set; } = 5;

    public string? CaptchaTurnstileSiteKey { get; set; }

    public string? CaptchaTurnstileSecretKey { get; set; }

    public string? CaptchaRecaptchaSiteKey { get; set; }

    public string? CaptchaRecaptchaSecretKey { get; set; }

    public string CaptchaRecaptchaVersion { get; set; } = "v2checkbox";

    public IReadOnlyList<VerificationDeliveryChannel> AvailableChannels { get; set; } =
        new List<VerificationDeliveryChannel>();
}

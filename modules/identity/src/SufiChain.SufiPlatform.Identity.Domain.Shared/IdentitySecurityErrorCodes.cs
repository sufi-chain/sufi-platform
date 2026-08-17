namespace SufiChain.SufiPlatform.Identity;

public static class IdentitySecurityErrorCodes
{
    public const string CaptchaValidationFailed = "Sufi.Identity:CaptchaValidationFailed";
    public const string SelfRegistrationDisabled = "Sufi.Identity:SelfRegistrationDisabled";
    public const string EmailConfirmationRequired = "Sufi.Identity:EmailConfirmationRequired";
    public const string OtpDisabled = "Sufi.Identity:OtpDisabled";
    public const string OtpRateLimitExceeded = "Sufi.Identity:OtpRateLimitExceeded";
    public const string OtpInvalidOrExpired = "Sufi.Identity:OtpInvalidOrExpired";
    public const string VerificationChannelUnavailable = "Sufi.Identity:VerificationChannelUnavailable";
    public const string PhoneNumberRequired = "Sufi.Identity:PhoneNumberRequired";
    public const string PhoneNumberNotConfirmed = "Sufi.Identity:PhoneNumberNotConfirmed";
    public const string TwoFactorCodeInvalid = "Sufi.Identity:TwoFactorCodeInvalid";
    public const string TwoFactorNotEnabled = "Sufi.Identity:TwoFactorNotEnabled";
    public const string TwoFactorSetupFailed = "Sufi.Identity:TwoFactorSetupFailed";
    public const string AuthenticatorKeyStoreUnavailable = "Sufi.Identity:AuthenticatorKeyStoreUnavailable";
    public const string TwoFactorCodeDeliveryDisabled = "Sufi.Identity:TwoFactorCodeDeliveryDisabled";
    public const string TwoFactorPendingLoginExpired = "Sufi.Identity:TwoFactorPendingLoginExpired";
    public const string TwoFactorChangeNotAllowed = "Sufi.Identity:TwoFactorChangeNotAllowed";
    public const string InvalidPassword = "Sufi.Identity:InvalidPassword";
    public const string UserNotAuthenticated = "Sufi.Identity:UserNotAuthenticated";
    public const string UserNotFound = "Sufi.Identity:UserNotFound";
    public const string AuthenticationNotAvailable = "Sufi.Identity:AuthenticationNotAvailable";
}

namespace SufiChain.SufiAbp.Identity;

public static class IdentitySecurityErrorCodes
{
    public const string CaptchaValidationFailed = "SufiAbp.Identity:CaptchaValidationFailed";
    public const string SelfRegistrationDisabled = "SufiAbp.Identity:SelfRegistrationDisabled";
    public const string EmailConfirmationRequired = "SufiAbp.Identity:EmailConfirmationRequired";
    public const string OtpDisabled = "SufiAbp.Identity:OtpDisabled";
    public const string OtpRateLimitExceeded = "SufiAbp.Identity:OtpRateLimitExceeded";
    public const string OtpInvalidOrExpired = "SufiAbp.Identity:OtpInvalidOrExpired";
    public const string VerificationChannelUnavailable = "SufiAbp.Identity:VerificationChannelUnavailable";
    public const string PhoneNumberRequired = "SufiAbp.Identity:PhoneNumberRequired";
    public const string PhoneNumberNotConfirmed = "SufiAbp.Identity:PhoneNumberNotConfirmed";
    public const string TwoFactorCodeInvalid = "SufiAbp.Identity:TwoFactorCodeInvalid";
    public const string TwoFactorNotEnabled = "SufiAbp.Identity:TwoFactorNotEnabled";
    public const string TwoFactorSetupFailed = "SufiAbp.Identity:TwoFactorSetupFailed";
    public const string TwoFactorCodeDeliveryDisabled = "SufiAbp.Identity:TwoFactorCodeDeliveryDisabled";
    public const string TwoFactorPendingLoginExpired = "SufiAbp.Identity:TwoFactorPendingLoginExpired";
    public const string TwoFactorChangeNotAllowed = "SufiAbp.Identity:TwoFactorChangeNotAllowed";
    public const string InvalidPassword = "SufiAbp.Identity:InvalidPassword";
    public const string UserNotAuthenticated = "SufiAbp.Identity:UserNotAuthenticated";
    public const string UserNotFound = "SufiAbp.Identity:UserNotFound";
    public const string AuthenticationNotAvailable = "SufiAbp.Identity:AuthenticationNotAvailable";
}

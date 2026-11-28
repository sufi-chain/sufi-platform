using SufiChain.SufiPlatform.Identity;

namespace SufiChain.SufiPlatform.Account;

public static class EmailConfirmationResend
{
    public const string CaptchaMessageKey = "EmailConfirmationCaptchaFailed";
    public const string GenericMessageKey = "EmailConfirmationResendFailed";

    public static string MessageKey(string? errorCode) =>
        string.Equals(errorCode, IdentitySecurityErrorCodes.CaptchaValidationFailed, StringComparison.Ordinal)
            ? CaptchaMessageKey
            : GenericMessageKey;

    public static bool NeedsFreshCaptcha(string? errorCode) =>
        string.Equals(errorCode, IdentitySecurityErrorCodes.CaptchaValidationFailed, StringComparison.Ordinal);
}

using SufiChain.SufiPlatform.Identity;
using Volo.Abp.ExceptionHandling;

namespace SufiChain.SufiPlatform.Account.Blazor.Pages;

/// <summary>
/// Forgot-password failures. A bad CAPTCHA is the only case with its own message.
/// Unknown and known emails stay on the same success response.
/// </summary>
public static class ForgotPasswordFeedback
{
    public const string CaptchaErrorKey = IdentitySecurityErrorCodes.CaptchaValidationFailed;

    public static bool IsCaptchaFailure(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is IHasErrorCode hasCode &&
                string.Equals(hasCode.Code, CaptchaErrorKey, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}

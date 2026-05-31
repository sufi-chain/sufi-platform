namespace SufiChain.SufiAbp.Captcha;

/// <summary>
/// Identifies the user flow that requires captcha validation.
/// </summary>
public enum CaptchaPurpose
{
    Register,

    Login,

    ForgotPassword,

    OtpSend,

    EmailConfirmationResend
}

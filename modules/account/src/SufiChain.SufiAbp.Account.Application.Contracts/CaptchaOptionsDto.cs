namespace SufiChain.SufiAbp.Account;

public class CaptchaOptionsDto
{
    public bool IsEnabled { get; set; }

    public string Provider { get; set; } = string.Empty;

    public string? SiteKey { get; set; }

    public bool RequiredOnRegister { get; set; }

    public bool RequiredOnForgotPassword { get; set; }

    public bool RequiredOnOtpSend { get; set; }

    public bool RequiredOnLogin { get; set; }

    public bool RequiredOnEmailConfirmationResend { get; set; }
}

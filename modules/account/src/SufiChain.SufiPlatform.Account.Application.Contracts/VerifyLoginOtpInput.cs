namespace SufiChain.SufiPlatform.Account;

public class VerifyLoginOtpInput : VerifyOtpInput
{
    public string? ReturnUrl { get; set; }

    public bool RememberMe { get; set; }
}

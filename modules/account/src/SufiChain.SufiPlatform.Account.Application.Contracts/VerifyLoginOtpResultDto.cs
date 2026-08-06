namespace SufiChain.SufiPlatform.Account;

public class VerifyLoginOtpResultDto
{
    public string LoginCompletionToken { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

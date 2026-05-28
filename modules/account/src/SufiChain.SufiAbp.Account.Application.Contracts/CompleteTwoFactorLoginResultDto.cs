namespace SufiChain.SufiAbp.Account;

public class CompleteTwoFactorLoginResultDto
{
    public string LoginCompletionToken { get; set; } = string.Empty;

    public string? ReturnUrl { get; set; }
}

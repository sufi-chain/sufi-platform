namespace SufiChain.SufiPlatform.Account;

public class CaptchaInputDto
{
    public string? CaptchaChallengeId { get; set; }

    public string? CaptchaAnswer { get; set; }

    public string? CaptchaToken { get; set; }
}

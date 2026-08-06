namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Challenge payload returned to clients for captcha completion.
/// </summary>
public class CaptchaChallenge
{
    public string? ChallengeId { get; set; }

    public string ProviderName { get; set; } = string.Empty;

    public string? Question { get; set; }

    public string? SiteKey { get; set; }
}

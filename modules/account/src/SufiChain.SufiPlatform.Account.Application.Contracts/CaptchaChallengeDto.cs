namespace SufiChain.SufiPlatform.Account;

public class CaptchaChallengeDto
{
    public string ChallengeId { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public string? Question { get; set; }

    public string? SiteKey { get; set; }
}

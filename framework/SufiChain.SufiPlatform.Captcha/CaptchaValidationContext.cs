namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Captcha validation input for a specific user flow.
/// </summary>
public class CaptchaValidationContext
{
    public CaptchaPurpose Purpose { get; set; }

    public string? ChallengeId { get; set; }

    public string? Answer { get; set; }

    public string? Token { get; set; }

    public string? RemoteIp { get; set; }
}

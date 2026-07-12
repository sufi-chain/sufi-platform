namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Cached answer hash for simple math captcha challenges.
/// </summary>
public class SimpleMathCaptchaCacheItem
{
    public string AnswerHash { get; set; } = string.Empty;
}

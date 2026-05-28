namespace SufiChain.SufiAbp.Captcha;

/// <summary>
/// Known captcha provider names stored in identity settings.
/// </summary>
public static class CaptchaProviderNames
{
    public const string Simple = "Simple";

    public const string Turnstile = "Turnstile";

    public const string Recaptcha = "Recaptcha";
}

namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Facade that applies identity captcha settings before delegating to a provider.
/// </summary>
public interface ICaptchaValidator
{
    Task<CaptchaValidationResult> ValidateAsync(
        CaptchaValidationContext context,
        CancellationToken cancellationToken = default);
}

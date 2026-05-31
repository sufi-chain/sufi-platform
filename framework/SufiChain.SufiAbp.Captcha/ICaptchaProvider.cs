namespace SufiChain.SufiAbp.Captcha;

/// <summary>
/// Captcha provider implementation contract.
/// </summary>
public interface ICaptchaProvider
{
    string Name { get; }

    Task<CaptchaChallenge> GenerateChallengeAsync(CancellationToken cancellationToken = default);

    Task<CaptchaValidationResult> ValidateAsync(
        CaptchaValidationContext context,
        CancellationToken cancellationToken = default);
}

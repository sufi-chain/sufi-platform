using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Captcha;

/// <summary>
/// No-op captcha provider used when captcha is disabled or not applicable.
/// </summary>
public class NullCaptchaProvider : ICaptchaProvider, ITransientDependency
{
    public string Name => "Null";

    public Task<CaptchaChallenge> GenerateChallengeAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new CaptchaChallenge
        {
            ProviderName = Name
        });
    }

    public Task<CaptchaValidationResult> ValidateAsync(
        CaptchaValidationContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CaptchaValidationResult.Success());
    }
}

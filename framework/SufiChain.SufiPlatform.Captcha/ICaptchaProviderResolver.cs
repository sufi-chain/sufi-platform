namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Resolves the active captcha provider from identity settings.
/// </summary>
public interface ICaptchaProviderResolver
{
    Task<ICaptchaProvider> ResolveAsync(CancellationToken cancellationToken = default);
}

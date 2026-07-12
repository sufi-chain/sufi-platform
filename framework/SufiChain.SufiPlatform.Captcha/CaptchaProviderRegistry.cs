using Volo.Abp;

namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Lookup table for registered captcha providers.
/// </summary>
public class CaptchaProviderRegistry
{
    private readonly IReadOnlyDictionary<string, ICaptchaProvider> _providers;

    public CaptchaProviderRegistry(IEnumerable<ICaptchaProvider> providers)
    {
        _providers = providers.ToDictionary(
            provider => provider.Name,
            provider => provider,
            StringComparer.OrdinalIgnoreCase);
    }

    public ICaptchaProvider GetRequiredProvider(string name)
    {
        if (_providers.TryGetValue(name, out var provider))
        {
            return provider;
        }

        throw new BusinessException(CaptchaErrorCodes.ProviderNotFound)
            .WithData("ProviderName", name);
    }

    public ICaptchaProvider? FindProvider(string name)
    {
        _providers.TryGetValue(name, out var provider);
        return provider;
    }
}

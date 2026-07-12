using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Resolves captcha providers using <see cref="IdentitySettingNames.Captcha.Provider"/>.
/// </summary>
public class CaptchaProviderResolver : ICaptchaProviderResolver, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    protected CaptchaProviderRegistry Registry { get; }

    public CaptchaProviderResolver(
        ISettingProvider settingProvider,
        CaptchaProviderRegistry registry)
    {
        SettingProvider = settingProvider;
        Registry = registry;
    }

    public virtual async Task<ICaptchaProvider> ResolveAsync(CancellationToken cancellationToken = default)
    {
        var providerName = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Provider);
        providerName = string.IsNullOrWhiteSpace(providerName)
            ? CaptchaProviderNames.Simple
            : providerName;

        var provider = Registry.FindProvider(providerName);
        if (provider != null)
        {
            return provider;
        }

        return Registry.GetRequiredProvider(CaptchaProviderNames.Simple);
    }
}

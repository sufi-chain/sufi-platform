using System;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Captcha;
using SufiChain.SufiAbp.Identity.Settings;
using Volo.Abp.Application.Services;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Account;

public class AccountCaptchaAppService : ApplicationService, ICaptchaAppService
{
    protected ICaptchaProviderResolver ProviderResolver { get; }

    protected ISettingProvider SettingProvider { get; }

    public AccountCaptchaAppService(
        ICaptchaProviderResolver providerResolver,
        ISettingProvider settingProvider)
    {
        ProviderResolver = providerResolver;
        SettingProvider = settingProvider;
    }

    public virtual async Task<CaptchaChallengeDto> GetChallengeAsync()
    {
        var provider = await ProviderResolver.ResolveAsync();

        if (string.Equals(provider.Name, CaptchaProviderNames.Turnstile, StringComparison.OrdinalIgnoreCase))
        {
            return new CaptchaChallengeDto
            {
                Provider = provider.Name,
                SiteKey = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Turnstile.SiteKey)
            };
        }

        if (string.Equals(provider.Name, CaptchaProviderNames.Recaptcha, StringComparison.OrdinalIgnoreCase))
        {
            return new CaptchaChallengeDto
            {
                Provider = provider.Name,
                SiteKey = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Recaptcha.SiteKey)
            };
        }

        var challenge = await provider.GenerateChallengeAsync();

        return new CaptchaChallengeDto
        {
            ChallengeId = challenge.ChallengeId ?? string.Empty,
            Provider = challenge.ProviderName,
            Question = challenge.Question,
            SiteKey = challenge.SiteKey
        };
    }

    public virtual async Task<CaptchaOptionsDto> GetOptionsAsync()
    {
        var provider = await ProviderResolver.ResolveAsync();
        string? siteKey = null;

        if (string.Equals(provider.Name, CaptchaProviderNames.Turnstile, StringComparison.OrdinalIgnoreCase))
        {
            siteKey = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Turnstile.SiteKey);
        }
        else if (string.Equals(provider.Name, CaptchaProviderNames.Recaptcha, StringComparison.OrdinalIgnoreCase))
        {
            siteKey = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Recaptcha.SiteKey);
        }

        return new CaptchaOptionsDto
        {
            IsEnabled = await SettingProvider.IsTrueAsync(IdentitySettingNames.Captcha.IsEnabled),
            Provider = provider.Name,
            SiteKey = siteKey,
            RequiredOnRegister = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.Captcha.RequiredOnRegister),
            RequiredOnForgotPassword = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.Captcha.RequiredOnForgotPassword),
            RequiredOnOtpSend = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.Captcha.RequiredOnOtpSend),
            RequiredOnLogin = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.Captcha.RequiredOnLogin),
            RequiredOnEmailConfirmationResend = await SettingProvider.IsTrueAsync(
                IdentitySettingNames.Captcha.RequiredOnEmailConfirmationResend)
        };
    }
}

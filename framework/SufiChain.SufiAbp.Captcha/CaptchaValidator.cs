using SufiChain.SufiAbp.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiAbp.Captcha;

/// <summary>
/// Applies captcha enablement and per-purpose requirements before provider validation.
/// </summary>
public class CaptchaValidator : ICaptchaValidator, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    protected ICaptchaProviderResolver ProviderResolver { get; }

    public CaptchaValidator(
        ISettingProvider settingProvider,
        ICaptchaProviderResolver providerResolver)
    {
        SettingProvider = settingProvider;
        ProviderResolver = providerResolver;
    }

    public virtual async Task<CaptchaValidationResult> ValidateAsync(
        CaptchaValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (!await IsEnabledAsync())
        {
            return CaptchaValidationResult.Success();
        }

        if (!await IsRequiredForPurposeAsync(context.Purpose))
        {
            return CaptchaValidationResult.Success();
        }

        var provider = await ProviderResolver.ResolveAsync(cancellationToken);
        return await provider.ValidateAsync(context, cancellationToken);
    }

    protected virtual async Task<bool> IsEnabledAsync()
    {
        return bool.Parse(
            await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.IsEnabled) ?? bool.TrueString);
    }

    protected virtual async Task<bool> IsRequiredForPurposeAsync(CaptchaPurpose purpose)
    {
        var settingName = purpose switch
        {
            CaptchaPurpose.Register => IdentitySettingNames.Captcha.RequiredOnRegister,
            CaptchaPurpose.Login => IdentitySettingNames.Captcha.RequiredOnLogin,
            CaptchaPurpose.ForgotPassword => IdentitySettingNames.Captcha.RequiredOnForgotPassword,
            CaptchaPurpose.OtpSend => IdentitySettingNames.Captcha.RequiredOnOtpSend,
            CaptchaPurpose.EmailConfirmationResend => IdentitySettingNames.Captcha.RequiredOnEmailConfirmationResend,
            _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, null)
        };

        return bool.Parse(await SettingProvider.GetOrNullAsync(settingName) ?? bool.FalseString);
    }
}

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Captcha.Recaptcha;

/// <summary>
/// Google reCAPTCHA v2 checkbox captcha provider using the siteverify API.
/// </summary>
public class RecaptchaCaptchaProvider : ICaptchaProvider, ITransientDependency
{
    public const string HttpClientName = "Sufi.Captcha.Recaptcha";

    private const string SiteVerifyUrl = "https://www.google.com/recaptcha/api/siteverify";

    protected IHttpClientFactory HttpClientFactory { get; }

    protected ISettingProvider SettingProvider { get; }

    public string Name => CaptchaProviderNames.Recaptcha;

    public RecaptchaCaptchaProvider(
        IHttpClientFactory httpClientFactory,
        ISettingProvider settingProvider)
    {
        HttpClientFactory = httpClientFactory;
        SettingProvider = settingProvider;
    }

    public virtual async Task<CaptchaChallenge> GenerateChallengeAsync(CancellationToken cancellationToken = default)
    {
        var siteKey = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Recaptcha.SiteKey);

        return new CaptchaChallenge
        {
            ProviderName = Name,
            SiteKey = siteKey
        };
    }

    public virtual async Task<CaptchaValidationResult> ValidateAsync(
        CaptchaValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.Token))
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        var secretKey = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Recaptcha.SecretKey);
        if (string.IsNullOrWhiteSpace(secretKey))
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        var version = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.Recaptcha.Version);
        if (!string.IsNullOrWhiteSpace(version) &&
            !string.Equals(version, "v2checkbox", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(version, "v2", StringComparison.OrdinalIgnoreCase))
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        using var requestContent = new FormUrlEncodedContent(BuildVerifyForm(context, secretKey));

        var httpClient = HttpClientFactory.CreateClient(HttpClientName);
        using var response = await httpClient.PostAsync(SiteVerifyUrl, requestContent, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        var payload = await response.Content.ReadFromJsonAsync<RecaptchaSiteVerifyResponse>(cancellationToken: cancellationToken);
        return payload?.Success == true
            ? CaptchaValidationResult.Success()
            : CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
    }

    private static Dictionary<string, string> BuildVerifyForm(CaptchaValidationContext context, string secretKey)
    {
        var form = new Dictionary<string, string>
        {
            ["secret"] = secretKey,
            ["response"] = context.Token!
        };

        if (!string.IsNullOrWhiteSpace(context.RemoteIp))
        {
            form["remoteip"] = context.RemoteIp;
        }

        return form;
    }

    private sealed class RecaptchaSiteVerifyResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp;

namespace SufiChain.SufiPlatform.Settings;

[Authorize(SettingsPermissions.Identity)]
public class IdentitySettingsAppService : SettingsAppServiceBase, IIdentitySettingsAppService
{
    protected ISettingManager SettingManager { get; }

    protected IVerificationChannelAvailabilityChecker? ChannelAvailabilityChecker { get; }

    public IdentitySettingsAppService(
        ISettingManager settingManager,
        IVerificationChannelAvailabilityChecker? channelAvailabilityChecker = null)
    {
        SettingManager = settingManager;
        ChannelAvailabilityChecker = channelAvailabilityChecker;
    }

    public virtual async Task<IdentitySettingsDto> GetAsync()
    {
        await CheckFeatureAsync();

        var dto = new IdentitySettingsDto
        {
            EnableSelfRegistration = await GetBoolAsync(IdentitySettingNames.Registration.EnableSelfRegistration, true),
            RequireEmailConfirmation = await GetBoolAsync(IdentitySettingNames.Registration.RequireEmailConfirmation),
            RequireConfirmedAccount = await GetBoolAsync(IdentitySettingNames.Registration.RequireConfirmedAccount),
            RequireConfirmedEmail = await GetBoolAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail),
            RequireConfirmedPhoneNumber = await GetBoolAsync(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber),
            RequireUniqueEmail = await GetBoolAsync(IdentitySettingNames.User.RequireUniqueEmail, true),
            PasswordRequiredLength = await GetIntAsync(IdentitySettingNames.Password.RequiredLength, 6),
            PasswordRequiredUniqueChars = await GetIntAsync(IdentitySettingNames.Password.RequiredUniqueChars, 1),
            PasswordRequireNonAlphanumeric = await GetBoolAsync(IdentitySettingNames.Password.RequireNonAlphanumeric),
            PasswordRequireLowercase = await GetBoolAsync(IdentitySettingNames.Password.RequireLowercase),
            PasswordRequireUppercase = await GetBoolAsync(IdentitySettingNames.Password.RequireUppercase),
            PasswordRequireDigit = await GetBoolAsync(IdentitySettingNames.Password.RequireDigit),
            PasswordDisallowUsername = await GetBoolAsync(IdentitySettingNames.Password.DisallowUsername),
            PasswordDisallowEmail = await GetBoolAsync(IdentitySettingNames.Password.DisallowEmail),
            PasswordMinimumAgeMinutes = await GetIntAsync(IdentitySettingNames.Password.MinimumAgeMinutes),
            LockoutAllowedForNewUsers = await GetBoolAsync(IdentitySettingNames.Lockout.AllowedForNewUsers, true),
            LockoutMaxFailedAccessAttempts = await GetIntAsync(IdentitySettingNames.Lockout.MaxFailedAccessAttempts, 5),
            LockoutDefaultLockoutTimeSpanMinutes = await GetIntAsync(
                IdentitySettingNames.Lockout.DefaultLockoutTimeSpanMinutes, 5),
            EmailConfirmationTokenLifespanHours = await GetIntAsync(
                IdentitySettingNames.Tokens.EmailConfirmationTokenLifespanHours, 24),
            PasswordResetTokenLifespanHours = await GetIntAsync(
                IdentitySettingNames.Tokens.PasswordResetTokenLifespanHours, 1),
            OtpTokenLifespanMinutes = await GetIntAsync(IdentitySettingNames.Tokens.OtpTokenLifespanMinutes, 5),
            OtpLength = await GetIntAsync(IdentitySettingNames.Tokens.OtpLength, 6),
            TwoFactorEnabled = await GetBoolAsync(IdentitySettingNames.TwoFactor.Enabled, true),
            TwoFactorIsRequired = await GetBoolAsync(IdentitySettingNames.TwoFactor.IsRequired),
            TwoFactorUsersCanChange = await GetBoolAsync(IdentitySettingNames.TwoFactor.UsersCanChange, true),
            TwoFactorEnforceForNewUsers = await GetBoolAsync(IdentitySettingNames.TwoFactor.EnforceForNewUsers),
            TwoFactorEnforceForAdministrators = await GetBoolAsync(
                IdentitySettingNames.TwoFactor.EnforceForAdministrators),
            TwoFactorAllowAuthenticatorApp = await GetBoolAsync(
                IdentitySettingNames.TwoFactor.AllowAuthenticatorApp, true),
            TwoFactorAllowCodeDelivery = await GetBoolAsync(IdentitySettingNames.TwoFactor.AllowCodeDelivery),
            TwoFactorCodeDeliveryChannel = await GetStringAsync(
                IdentitySettingNames.TwoFactor.CodeDeliveryChannel, "Email"),
            OtpIsEnabled = await GetBoolAsync(IdentitySettingNames.Otp.IsEnabled),
            OtpAllowRegistration = await GetBoolAsync(IdentitySettingNames.Otp.AllowRegistration),
            OtpAllowLogin = await GetBoolAsync(IdentitySettingNames.Otp.AllowLogin),
            OtpDefaultChannel = await GetStringAsync(IdentitySettingNames.Otp.DefaultChannel, "Email"),
            OtpMaxAttemptsPerCode = await GetIntAsync(IdentitySettingNames.Otp.MaxAttemptsPerCode, 5),
            OtpRateLimitPerIdentifierPerHour = await GetIntAsync(
                IdentitySettingNames.Otp.RateLimitPerIdentifierPerHour, 10),
            CaptchaIsEnabled = await GetBoolAsync(IdentitySettingNames.Captcha.IsEnabled, true),
            CaptchaProvider = await GetStringAsync(IdentitySettingNames.Captcha.Provider, "Simple"),
            CaptchaRequiredOnRegister = await GetBoolAsync(IdentitySettingNames.Captcha.RequiredOnRegister, true),
            CaptchaRequiredOnForgotPassword = await GetBoolAsync(
                IdentitySettingNames.Captcha.RequiredOnForgotPassword, true),
            CaptchaRequiredOnOtpSend = await GetBoolAsync(IdentitySettingNames.Captcha.RequiredOnOtpSend, true),
            CaptchaRequiredOnLogin = await GetBoolAsync(IdentitySettingNames.Captcha.RequiredOnLogin),
            CaptchaRequiredOnEmailConfirmationResend = await GetBoolAsync(
                IdentitySettingNames.Captcha.RequiredOnEmailConfirmationResend, true),
            CaptchaChallengeExpirationMinutes = await GetIntAsync(
                IdentitySettingNames.Captcha.ChallengeExpirationMinutes, 5),
            CaptchaTurnstileSiteKey = await GetStringAsync(IdentitySettingNames.Captcha.Turnstile.SiteKey),
            CaptchaRecaptchaSiteKey = await GetStringAsync(IdentitySettingNames.Captcha.Recaptcha.SiteKey),
            CaptchaRecaptchaVersion = await GetStringAsync(
                IdentitySettingNames.Captcha.Recaptcha.Version, "v2checkbox")
        };

        ApplyChannelFlags(
            dto,
            await GetStringAsync(IdentitySettingNames.TwoFactor.AllowedCodeChannels, "Email"),
            await GetStringAsync(IdentitySettingNames.Otp.AllowedChannels, "Email"));

        dto.AvailableChannels = ChannelAvailabilityChecker != null
            ? await ChannelAvailabilityChecker.GetAvailableChannelsAsync()
            : new List<VerificationDeliveryChannel> { VerificationDeliveryChannel.Email };

        return dto;
    }

    public virtual async Task UpdateAsync(UpdateIdentitySettingsDto input)
    {
        await CheckFeatureAsync();

        await SetBoolAsync(IdentitySettingNames.Registration.EnableSelfRegistration, input.EnableSelfRegistration);
        await SetBoolAsync(IdentitySettingNames.Registration.RequireEmailConfirmation, input.RequireEmailConfirmation);
        await SetBoolAsync(IdentitySettingNames.Registration.RequireConfirmedAccount, input.RequireConfirmedAccount);
        await SetBoolAsync(IdentitySettingNames.SignIn.RequireConfirmedEmail, input.RequireConfirmedEmail);
        await SetBoolAsync(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber, input.RequireConfirmedPhoneNumber);
        await SetBoolAsync(IdentitySettingNames.User.RequireUniqueEmail, input.RequireUniqueEmail);
        await SetIntAsync(IdentitySettingNames.Password.RequiredLength, input.PasswordRequiredLength);
        await SetIntAsync(IdentitySettingNames.Password.RequiredUniqueChars, input.PasswordRequiredUniqueChars);
        await SetBoolAsync(IdentitySettingNames.Password.RequireNonAlphanumeric, input.PasswordRequireNonAlphanumeric);
        await SetBoolAsync(IdentitySettingNames.Password.RequireLowercase, input.PasswordRequireLowercase);
        await SetBoolAsync(IdentitySettingNames.Password.RequireUppercase, input.PasswordRequireUppercase);
        await SetBoolAsync(IdentitySettingNames.Password.RequireDigit, input.PasswordRequireDigit);
        await SetBoolAsync(IdentitySettingNames.Password.DisallowUsername, input.PasswordDisallowUsername);
        await SetBoolAsync(IdentitySettingNames.Password.DisallowEmail, input.PasswordDisallowEmail);
        await SetIntAsync(IdentitySettingNames.Password.MinimumAgeMinutes, input.PasswordMinimumAgeMinutes);
        await SetBoolAsync(IdentitySettingNames.Lockout.AllowedForNewUsers, input.LockoutAllowedForNewUsers);
        await SetIntAsync(IdentitySettingNames.Lockout.MaxFailedAccessAttempts, input.LockoutMaxFailedAccessAttempts);
        await SetIntAsync(
            IdentitySettingNames.Lockout.DefaultLockoutTimeSpanMinutes,
            input.LockoutDefaultLockoutTimeSpanMinutes);
        await SetIntAsync(
            IdentitySettingNames.Tokens.EmailConfirmationTokenLifespanHours,
            input.EmailConfirmationTokenLifespanHours);
        await SetIntAsync(
            IdentitySettingNames.Tokens.PasswordResetTokenLifespanHours,
            input.PasswordResetTokenLifespanHours);
        await SetIntAsync(IdentitySettingNames.Tokens.OtpTokenLifespanMinutes, input.OtpTokenLifespanMinutes);
        await SetIntAsync(IdentitySettingNames.Tokens.OtpLength, input.OtpLength);
        await SetBoolAsync(IdentitySettingNames.TwoFactor.Enabled, input.TwoFactorEnabled);
        await SetBoolAsync(IdentitySettingNames.TwoFactor.IsRequired, input.TwoFactorIsRequired);
        await SetBoolAsync(IdentitySettingNames.TwoFactor.UsersCanChange, input.TwoFactorUsersCanChange);
        await SetBoolAsync(IdentitySettingNames.TwoFactor.EnforceForNewUsers, input.TwoFactorEnforceForNewUsers);
        await SetBoolAsync(
            IdentitySettingNames.TwoFactor.EnforceForAdministrators,
            input.TwoFactorEnforceForAdministrators);
        await SetBoolAsync(IdentitySettingNames.TwoFactor.AllowAuthenticatorApp, input.TwoFactorAllowAuthenticatorApp);
        await SetBoolAsync(IdentitySettingNames.TwoFactor.AllowCodeDelivery, input.TwoFactorAllowCodeDelivery);
        await SetStringAsync(IdentitySettingNames.TwoFactor.CodeDeliveryChannel, input.TwoFactorCodeDeliveryChannel);
        await SetStringAsync(
            IdentitySettingNames.TwoFactor.AllowedCodeChannels,
            BuildChannelList(input.TwoFactorAllowEmailChannel, input.TwoFactorAllowSmsChannel, input.TwoFactorAllowVoiceChannel));
        await SetBoolAsync(IdentitySettingNames.Otp.IsEnabled, input.OtpIsEnabled);
        await SetBoolAsync(IdentitySettingNames.Otp.AllowRegistration, input.OtpAllowRegistration);
        await SetBoolAsync(IdentitySettingNames.Otp.AllowLogin, input.OtpAllowLogin);
        await SetStringAsync(IdentitySettingNames.Otp.DefaultChannel, input.OtpDefaultChannel);
        await SetStringAsync(
            IdentitySettingNames.Otp.AllowedChannels,
            BuildChannelList(input.OtpAllowEmailChannel, input.OtpAllowSmsChannel, input.OtpAllowVoiceChannel));
        await SetIntAsync(IdentitySettingNames.Otp.MaxAttemptsPerCode, input.OtpMaxAttemptsPerCode);
        await SetIntAsync(
            IdentitySettingNames.Otp.RateLimitPerIdentifierPerHour,
            input.OtpRateLimitPerIdentifierPerHour);
        await SetBoolAsync(IdentitySettingNames.Captcha.IsEnabled, input.CaptchaIsEnabled);
        await SetStringAsync(IdentitySettingNames.Captcha.Provider, input.CaptchaProvider);
        await SetBoolAsync(IdentitySettingNames.Captcha.RequiredOnRegister, input.CaptchaRequiredOnRegister);
        await SetBoolAsync(
            IdentitySettingNames.Captcha.RequiredOnForgotPassword,
            input.CaptchaRequiredOnForgotPassword);
        await SetBoolAsync(IdentitySettingNames.Captcha.RequiredOnOtpSend, input.CaptchaRequiredOnOtpSend);
        await SetBoolAsync(IdentitySettingNames.Captcha.RequiredOnLogin, input.CaptchaRequiredOnLogin);
        await SetBoolAsync(
            IdentitySettingNames.Captcha.RequiredOnEmailConfirmationResend,
            input.CaptchaRequiredOnEmailConfirmationResend);
        await SetIntAsync(
            IdentitySettingNames.Captcha.ChallengeExpirationMinutes,
            input.CaptchaChallengeExpirationMinutes);
        await SetStringAsync(IdentitySettingNames.Captcha.Turnstile.SiteKey, input.CaptchaTurnstileSiteKey);
        if (!string.IsNullOrWhiteSpace(input.CaptchaTurnstileSecretKey))
        {
            await SetStringAsync(IdentitySettingNames.Captcha.Turnstile.SecretKey, input.CaptchaTurnstileSecretKey);
        }

        await SetStringAsync(IdentitySettingNames.Captcha.Recaptcha.SiteKey, input.CaptchaRecaptchaSiteKey);
        if (!string.IsNullOrWhiteSpace(input.CaptchaRecaptchaSecretKey))
        {
            await SetStringAsync(IdentitySettingNames.Captcha.Recaptcha.SecretKey, input.CaptchaRecaptchaSecretKey);
        }

        await SetStringAsync(IdentitySettingNames.Captcha.Recaptcha.Version, input.CaptchaRecaptchaVersion);
    }

    protected virtual void ApplyChannelFlags(IdentitySettingsDto dto, string twoFactorChannels, string otpChannels)
    {
        dto.TwoFactorAllowEmailChannel = ContainsChannel(twoFactorChannels, "Email");
        dto.TwoFactorAllowSmsChannel = ContainsChannel(twoFactorChannels, "Sms");
        dto.TwoFactorAllowVoiceChannel = ContainsChannel(twoFactorChannels, "Voice");
        dto.OtpAllowEmailChannel = ContainsChannel(otpChannels, "Email");
        dto.OtpAllowSmsChannel = ContainsChannel(otpChannels, "Sms");
        dto.OtpAllowVoiceChannel = ContainsChannel(otpChannels, "Voice");
    }

    protected virtual string BuildChannelList(bool email, bool sms, bool voice)
    {
        var channels = new List<string>();
        if (email)
        {
            channels.Add("Email");
        }

        if (sms)
        {
            channels.Add("Sms");
        }

        if (voice)
        {
            channels.Add("Voice");
        }

        return channels.Count == 0 ? "Email" : string.Join(',', channels);
    }

    protected static bool ContainsChannel(string channels, string name)
    {
        return channels.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(c => string.Equals(c, name, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual async Task<string?> GetSettingValueOrNullAsync(string name)
    {
        return CurrentTenant.Id.HasValue
            ? await SettingManager.GetOrNullForTenantAsync(name, CurrentTenant.Id.Value, fallback: true)
            : await SettingManager.GetOrNullGlobalAsync(name);
    }

    protected virtual async Task<bool> GetBoolAsync(string name, bool defaultValue = false)
    {
        var value = await GetSettingValueOrNullAsync(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : bool.TryParse(value, out var result) && result;
    }

    protected virtual async Task<int> GetIntAsync(string name, int defaultValue = 0)
    {
        var value = await GetSettingValueOrNullAsync(name);
        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    protected virtual async Task<string> GetStringAsync(string name, string defaultValue = "")
    {
        var value = await GetSettingValueOrNullAsync(name);
        return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
    }

    protected virtual Task SetBoolAsync(string name, bool value)
    {
        return SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value.ToString().ToLowerInvariant());
    }

    protected virtual Task SetIntAsync(string name, int value)
    {
        return SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value.ToString());
    }

    protected virtual Task SetStringAsync(string name, string? value)
    {
        return SettingManager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value);
    }

    protected virtual async Task CheckFeatureAsync()
    {
        if (!await FeatureChecker.IsEnabledAsync(SettingsFeatures.Enable))
        {
            throw new BusinessException($"Feature is disabled: {SettingsFeatures.Enable}");
        }
    }
}

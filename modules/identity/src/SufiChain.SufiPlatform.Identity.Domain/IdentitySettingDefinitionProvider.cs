using Volo.Abp.Localization;
using SufiChain.SufiPlatform.Identity.Localization;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Identity;

public class IdentitySettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(IdentitySettingNames.Registration.EnableSelfRegistration, "true", L("Setting:EnableSelfRegistration")),
            new SettingDefinition(IdentitySettingNames.Registration.RequireEmailConfirmation, "false", L("Setting:RequireEmailConfirmation")),
            new SettingDefinition(IdentitySettingNames.Registration.RequireConfirmedAccount, "false", L("Setting:RequireConfirmedAccount")),
            new SettingDefinition(IdentitySettingNames.SignIn.RequireConfirmedEmail, "false", L("Setting:RequireConfirmedEmail")),
            new SettingDefinition(IdentitySettingNames.SignIn.RequireConfirmedPhoneNumber, "false", L("Setting:RequireConfirmedPhoneNumber")),
            new SettingDefinition(IdentitySettingNames.User.RequireUniqueEmail, "true", L("Setting:RequireUniqueEmail")),
            new SettingDefinition(IdentitySettingNames.Password.RequiredLength, "6", L("Setting:PasswordRequiredLength")),
            new SettingDefinition(IdentitySettingNames.Password.RequiredUniqueChars, "1", L("Setting:PasswordRequiredUniqueChars")),
            new SettingDefinition(IdentitySettingNames.Password.RequireNonAlphanumeric, "true", L("Setting:PasswordRequireNonAlphanumeric")),
            new SettingDefinition(IdentitySettingNames.Password.RequireLowercase, "true", L("Setting:PasswordRequireLowercase")),
            new SettingDefinition(IdentitySettingNames.Password.RequireUppercase, "true", L("Setting:PasswordRequireUppercase")),
            new SettingDefinition(IdentitySettingNames.Password.RequireDigit, "true", L("Setting:PasswordRequireDigit")),
            new SettingDefinition(IdentitySettingNames.Password.DisallowUsername, "true", L("Setting:PasswordDisallowUsername")),
            new SettingDefinition(IdentitySettingNames.Password.DisallowEmail, "true", L("Setting:PasswordDisallowEmail")),
            new SettingDefinition(IdentitySettingNames.Password.MinimumAgeMinutes, "0", L("Setting:PasswordMinimumAgeMinutes")),
            new SettingDefinition(IdentitySettingNames.Lockout.AllowedForNewUsers, "true", L("Setting:LockoutAllowedForNewUsers")),
            new SettingDefinition(IdentitySettingNames.Lockout.MaxFailedAccessAttempts, "5", L("Setting:LockoutMaxFailedAccessAttempts")),
            new SettingDefinition(IdentitySettingNames.Lockout.DefaultLockoutTimeSpanMinutes, "5", L("Setting:LockoutDefaultLockoutTimeSpanMinutes")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.Enabled, "true", L("Setting:TwoFactorEnabled")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.IsRequired, "false", L("Setting:TwoFactorIsRequired")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.UsersCanChange, "true", L("Setting:TwoFactorUsersCanChange")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.EnforceForNewUsers, "false", L("Setting:TwoFactorEnforceForNewUsers")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.EnforceForAdministrators, "false", L("Setting:TwoFactorEnforceForAdministrators")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.AllowAuthenticatorApp, "true", L("Setting:TwoFactorAllowAuthenticatorApp")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.AllowCodeDelivery, "false", L("Setting:TwoFactorAllowCodeDelivery")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.CodeDeliveryChannel, "Email", L("Setting:TwoFactorCodeDeliveryChannel")),
            new SettingDefinition(IdentitySettingNames.TwoFactor.AllowedCodeChannels, "Email", L("Setting:TwoFactorAllowedCodeChannels")),
            new SettingDefinition(IdentitySettingNames.Tokens.EmailConfirmationTokenLifespanHours, "24", L("Setting:EmailConfirmationTokenLifespanHours")),
            new SettingDefinition(IdentitySettingNames.Tokens.PasswordResetTokenLifespanHours, "1", L("Setting:PasswordResetTokenLifespanHours")),
            new SettingDefinition(IdentitySettingNames.Tokens.OtpTokenLifespanMinutes, "5", L("Setting:OtpTokenLifespanMinutes")),
            new SettingDefinition(IdentitySettingNames.Tokens.OtpLength, "6", L("Setting:OtpLength")),
            new SettingDefinition(IdentitySettingNames.Otp.IsEnabled, "false", L("Setting:OtpIsEnabled")),
            new SettingDefinition(IdentitySettingNames.Otp.AllowRegistration, "false", L("Setting:OtpAllowRegistration")),
            new SettingDefinition(IdentitySettingNames.Otp.AllowLogin, "false", L("Setting:OtpAllowLogin")),
            new SettingDefinition(IdentitySettingNames.Otp.DefaultChannel, "Email", L("Setting:OtpDefaultChannel")),
            new SettingDefinition(IdentitySettingNames.Otp.AllowedChannels, "Email", L("Setting:OtpAllowedChannels")),
            new SettingDefinition(IdentitySettingNames.Otp.MaxAttemptsPerCode, "3", L("Setting:OtpMaxAttemptsPerCode")),
            new SettingDefinition(IdentitySettingNames.Otp.RateLimitPerIdentifierPerHour, "10", L("Setting:OtpRateLimitPerIdentifierPerHour")),
            new SettingDefinition(IdentitySettingNames.Captcha.IsEnabled, "true", L("Setting:CaptchaIsEnabled")),
            new SettingDefinition(IdentitySettingNames.Captcha.Provider, "Simple", L("Setting:CaptchaProvider")),
            new SettingDefinition(IdentitySettingNames.Captcha.RequiredOnRegister, "true", L("Setting:CaptchaRequiredOnRegister")),
            new SettingDefinition(IdentitySettingNames.Captcha.RequiredOnForgotPassword, "true", L("Setting:CaptchaRequiredOnForgotPassword")),
            new SettingDefinition(IdentitySettingNames.Captcha.RequiredOnOtpSend, "true", L("Setting:CaptchaRequiredOnOtpSend")),
            new SettingDefinition(IdentitySettingNames.Captcha.RequiredOnLogin, "false", L("Setting:CaptchaRequiredOnLogin")),
            new SettingDefinition(IdentitySettingNames.Captcha.RequiredOnEmailConfirmationResend, "true", L("Setting:CaptchaRequiredOnEmailConfirmationResend")),
            new SettingDefinition(IdentitySettingNames.Captcha.ChallengeExpirationMinutes, "5", L("Setting:CaptchaChallengeExpirationMinutes")),
            new SettingDefinition(IdentitySettingNames.Captcha.Turnstile.SiteKey, "", L("Setting:CaptchaTurnstileSiteKey")),
            new SettingDefinition(IdentitySettingNames.Captcha.Turnstile.SecretKey, "", L("Setting:CaptchaTurnstileSecretKey"), isEncrypted: true),
            new SettingDefinition(IdentitySettingNames.Captcha.Recaptcha.SiteKey, "", L("Setting:CaptchaRecaptchaSiteKey")),
            new SettingDefinition(IdentitySettingNames.Captcha.Recaptcha.SecretKey, "", L("Setting:CaptchaRecaptchaSecretKey"), isEncrypted: true),
            new SettingDefinition(IdentitySettingNames.Captcha.Recaptcha.Version, "v2checkbox", L("Setting:CaptchaRecaptchaVersion")),
            new SettingDefinition(IdentitySettingNames.Captcha.Recaptcha.MinScore, "0.5", L("Setting:CaptchaRecaptchaMinScore"))
        );
    }

    private static LocalizableString L(string name) => LocalizableString.Create<SufiIdentityResource>(name);
}

namespace SufiChain.SufiAbp.Identity.Settings;

public static class IdentitySettingNames
{
    private const string Prefix = "SufiAbp.Identity";

    public static class SignIn
    {
        private const string SignInPrefix = Prefix + ".SignIn";

        public const string RequireConfirmedEmail = SignInPrefix + ".RequireConfirmedEmail";
        public const string RequireConfirmedPhoneNumber = SignInPrefix + ".RequireConfirmedPhoneNumber";
    }

    public static class User
    {
        private const string UserPrefix = Prefix + ".User";

        public const string AllowedUserNameCharacters = UserPrefix + ".AllowedUserNameCharacters";
        public const string RequireUniqueEmail = UserPrefix + ".RequireUniqueEmail";
    }

    public static class Password
    {
        private const string PasswordPrefix = Prefix + ".Password";

        public const string RequiredLength = PasswordPrefix + ".RequiredLength";
        public const string RequiredUniqueChars = PasswordPrefix + ".RequiredUniqueChars";
        public const string RequireNonAlphanumeric = PasswordPrefix + ".RequireNonAlphanumeric";
        public const string RequireLowercase = PasswordPrefix + ".RequireLowercase";
        public const string RequireUppercase = PasswordPrefix + ".RequireUppercase";
        public const string RequireDigit = PasswordPrefix + ".RequireDigit";
        public const string DisallowUsername = PasswordPrefix + ".DisallowUsername";
        public const string DisallowEmail = PasswordPrefix + ".DisallowEmail";
        public const string MinimumAgeMinutes = PasswordPrefix + ".MinimumAgeMinutes";
        public const string PasswordHistoryLimit = PasswordPrefix + ".PasswordHistoryLimit";
        public const string PasswordExpirationDays = PasswordPrefix + ".PasswordExpirationDays";
    }

    public static class Lockout
    {
        private const string LockoutPrefix = Prefix + ".Lockout";

        public const string AllowedForNewUsers = LockoutPrefix + ".AllowedForNewUsers";
        public const string MaxFailedAccessAttempts = LockoutPrefix + ".MaxFailedAccessAttempts";
        public const string DefaultLockoutTimeSpanMinutes = LockoutPrefix + ".DefaultLockoutTimeSpanMinutes";
    }

    public static class TwoFactor
    {
        private const string TwoFactorPrefix = Prefix + ".TwoFactor";

        public const string Enabled = TwoFactorPrefix + ".Enabled";
        public const string IsRequired = TwoFactorPrefix + ".IsRequired";
        public const string UsersCanChange = TwoFactorPrefix + ".UsersCanChange";
        public const string EnforceForNewUsers = TwoFactorPrefix + ".EnforceForNewUsers";
        public const string EnforceForAdministrators = TwoFactorPrefix + ".EnforceForAdministrators";
        public const string AllowAuthenticatorApp = TwoFactorPrefix + ".AllowAuthenticatorApp";
        public const string AllowCodeDelivery = TwoFactorPrefix + ".AllowCodeDelivery";
        public const string CodeDeliveryChannel = TwoFactorPrefix + ".CodeDeliveryChannel";
        public const string AllowedCodeChannels = TwoFactorPrefix + ".AllowedCodeChannels";
    }

    public static class Tokens
    {
        private const string TokensPrefix = Prefix + ".Tokens";

        public const string EmailConfirmationTokenLifespanHours = TokensPrefix + ".EmailConfirmationTokenLifespanHours";
        public const string PasswordResetTokenLifespanHours = TokensPrefix + ".PasswordResetTokenLifespanHours";
        public const string OtpTokenLifespanMinutes = TokensPrefix + ".OtpTokenLifespanMinutes";
        public const string OtpLength = TokensPrefix + ".OtpLength";
    }

    public static class Registration
    {
        private const string RegistrationPrefix = Prefix + ".Registration";

        public const string EnableSelfRegistration = RegistrationPrefix + ".EnableSelfRegistration";
        public const string RequireEmailConfirmation = RegistrationPrefix + ".RequireEmailConfirmation";
        public const string RequireConfirmedAccount = RegistrationPrefix + ".RequireConfirmedAccount";
    }

    public static class Otp
    {
        private const string OtpPrefix = Prefix + ".Otp";

        public const string IsEnabled = OtpPrefix + ".IsEnabled";
        public const string AllowRegistration = OtpPrefix + ".AllowRegistration";
        public const string AllowLogin = OtpPrefix + ".AllowLogin";
        public const string DefaultChannel = OtpPrefix + ".DefaultChannel";
        public const string AllowedChannels = OtpPrefix + ".AllowedChannels";
        public const string MaxAttemptsPerCode = OtpPrefix + ".MaxAttemptsPerCode";
        public const string RateLimitPerIdentifierPerHour = OtpPrefix + ".RateLimitPerIdentifierPerHour";
    }

    public static class Captcha
    {
        private const string CaptchaPrefix = Prefix + ".Captcha";

        public const string IsEnabled = CaptchaPrefix + ".IsEnabled";
        public const string Provider = CaptchaPrefix + ".Provider";
        public const string RequiredOnRegister = CaptchaPrefix + ".RequiredOnRegister";
        public const string RequiredOnForgotPassword = CaptchaPrefix + ".RequiredOnForgotPassword";
        public const string RequiredOnOtpSend = CaptchaPrefix + ".RequiredOnOtpSend";
        public const string RequiredOnLogin = CaptchaPrefix + ".RequiredOnLogin";
        public const string RequiredOnEmailConfirmationResend = CaptchaPrefix + ".RequiredOnEmailConfirmationResend";
        public const string ChallengeExpirationMinutes = CaptchaPrefix + ".ChallengeExpirationMinutes";

        public static class Turnstile
        {
            public const string SiteKey = CaptchaPrefix + ".Turnstile.SiteKey";
            public const string SecretKey = CaptchaPrefix + ".Turnstile.SecretKey";
        }

        public static class Recaptcha
        {
            public const string SiteKey = CaptchaPrefix + ".Recaptcha.SiteKey";
            public const string SecretKey = CaptchaPrefix + ".Recaptcha.SecretKey";
            public const string Version = CaptchaPrefix + ".Recaptcha.Version";
            public const string MinScore = CaptchaPrefix + ".Recaptcha.MinScore";
        }
    }
}

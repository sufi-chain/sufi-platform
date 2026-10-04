using Microsoft.Extensions.Localization;
using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Identity.Localization;

namespace SufiChain.SufiPlatform.Settings.Blazor.Settings;

public partial class IdentitySettingsGroup : SettingsComponentBase, IEditableSettingGroup
{
    private readonly SettingGroupEditor<IdentitySettingsDto> _edits;

    public IdentitySettingsGroup()
    {
        _edits = new SettingGroupEditor<IdentitySettingsDto>(() => _settings, value => _settings = value ?? new IdentitySettingsDto());
    }
    private const string SimpleCaptchaProvider = "Simple";
    private const string TurnstileCaptchaProvider = "Turnstile";
    private const string RecaptchaCaptchaProvider = "Recaptcha";

    private static class LoadingKeys
    {
        public const string Load = "load";
        public const string Save = "save";
    }

    private static readonly VerificationDeliveryChannel[] DeliveryChannels =
    [
        VerificationDeliveryChannel.Email,
        VerificationDeliveryChannel.Sms,
        VerificationDeliveryChannel.Voice
    ];

    private IIdentitySettingsAppService IdentitySettingsAppService =>
        LazyGetRequiredService(ref _identitySettingsAppService);

    private IStringLocalizer<SufiIdentityResource> IdentityL =>
        LazyGetRequiredService(ref _identityLocalizer);

    private IStringLocalizer<SufiAccountResource> AccountL =>
        LazyGetRequiredService(ref _accountLocalizer);

    private IIdentitySettingsAppService? _identitySettingsAppService;
    private IStringLocalizer<SufiIdentityResource>? _identityLocalizer;
    private IStringLocalizer<SufiAccountResource>? _accountLocalizer;

    private IdentitySettingsDto _settings = new();

    public bool IsSaving => IsOperationLoading(LoadingKeys.Save);

    public bool HasUnsavedChanges => _edits.HasUnsavedChanges;

    public event Action? EditStateChanged
    {
        add => _edits.Changed += value;
        remove => _edits.Changed -= value;
    }

    protected bool RequireConfirmedEmail
    {
        get => _settings.RequireConfirmedEmail;
        set
        {
            _settings.RequireConfirmedEmail = value;
            if (value)
            {
                _settings.RequireEmailConfirmation = true;
            }
        }
    }

    protected bool IsTurnstileCaptchaProvider =>
        string.Equals(_settings.CaptchaProvider, TurnstileCaptchaProvider, StringComparison.OrdinalIgnoreCase);

    protected bool IsRecaptchaCaptchaProvider =>
        string.Equals(_settings.CaptchaProvider, RecaptchaCaptchaProvider, StringComparison.OrdinalIgnoreCase);

    protected VerificationDeliveryChannel TwoFactorDefaultChannel
    {
        get => ParseDeliveryChannel(_settings.TwoFactorCodeDeliveryChannel);
        set => _settings.TwoFactorCodeDeliveryChannel = value.ToString();
    }

    protected VerificationDeliveryChannel OtpDefaultChannelValue
    {
        get => ParseDeliveryChannel(_settings.OtpDefaultChannel);
        set => _settings.OtpDefaultChannel = value.ToString();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        _edits.Observe();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        if (firstRender)
        {
            await LoadSettingsAsync();
        }
    }

    protected virtual bool IsChannelAvailable(VerificationDeliveryChannel channel)
    {
        return _settings.AvailableChannels.Contains(channel);
    }

    protected virtual string GetChannelLabel(VerificationDeliveryChannel channel) => channel switch
    {
        VerificationDeliveryChannel.Sms => AccountL["ChannelSms"],
        VerificationDeliveryChannel.Voice => AccountL["ChannelVoice"],
        _ => AccountL["ChannelEmail"]
    };

    private static VerificationDeliveryChannel ParseDeliveryChannel(string? value) =>
        Enum.TryParse<VerificationDeliveryChannel>(value, ignoreCase: true, out var channel)
            ? channel
            : VerificationDeliveryChannel.Email;

    private Task LoadSettingsAsync() => ExecuteWithLoadingAsync(async () =>
    {
        _settings = await IdentitySettingsAppService.GetAsync();
        _edits.Capture();
    }, LoadingKeys.Load);

    public async Task SaveAsync()
    {
        if (await TrySaveAsync())
        {
            await Notify.SuccessAsync(L["SettingsSavedSuccessfully"]);
        }
    }

    public async Task<bool> TrySaveAsync()
    {
        var saved = await ExecuteWithLoadingAsync(async () =>
        {
            await IdentitySettingsAppService.UpdateAsync(BuildUpdateDto());
            _settings = await IdentitySettingsAppService.GetAsync();
            _edits.Capture();
            return true;
        }, LoadingKeys.Save);

        return saved == true;
    }

    public Task DiscardAsync()
    {
        _edits.Restore();
        return InvokeAsync(StateHasChanged);
    }

    private UpdateIdentitySettingsDto BuildUpdateDto() => new()
    {
            EnableSelfRegistration = _settings.EnableSelfRegistration,
            RequireEmailConfirmation = _settings.RequireEmailConfirmation,
            RequireConfirmedEmail = _settings.RequireConfirmedEmail,
            RequireConfirmedPhoneNumber = _settings.RequireConfirmedPhoneNumber,
            RequireUniqueEmail = _settings.RequireUniqueEmail,
            PasswordRequiredLength = _settings.PasswordRequiredLength,
            PasswordRequiredUniqueChars = _settings.PasswordRequiredUniqueChars,
            PasswordRequireNonAlphanumeric = _settings.PasswordRequireNonAlphanumeric,
            PasswordRequireLowercase = _settings.PasswordRequireLowercase,
            PasswordRequireUppercase = _settings.PasswordRequireUppercase,
            PasswordRequireDigit = _settings.PasswordRequireDigit,
            PasswordDisallowUsername = _settings.PasswordDisallowUsername,
            PasswordDisallowEmail = _settings.PasswordDisallowEmail,
            PasswordMinimumAgeMinutes = _settings.PasswordMinimumAgeMinutes,
            LockoutAllowedForNewUsers = _settings.LockoutAllowedForNewUsers,
            LockoutMaxFailedAccessAttempts = _settings.LockoutMaxFailedAccessAttempts,
            LockoutDefaultLockoutTimeSpanMinutes = _settings.LockoutDefaultLockoutTimeSpanMinutes,
            EmailConfirmationTokenLifespanHours = _settings.EmailConfirmationTokenLifespanHours,
            PasswordResetTokenLifespanHours = _settings.PasswordResetTokenLifespanHours,
            OtpTokenLifespanMinutes = _settings.OtpTokenLifespanMinutes,
            OtpLength = _settings.OtpLength,
            TwoFactorEnabled = _settings.TwoFactorEnabled,
            TwoFactorIsRequired = _settings.TwoFactorIsRequired,
            TwoFactorUsersCanChange = _settings.TwoFactorUsersCanChange,
            TwoFactorEnforceForNewUsers = _settings.TwoFactorEnforceForNewUsers,
            TwoFactorEnforceForAdministrators = _settings.TwoFactorEnforceForAdministrators,
            TwoFactorAllowAuthenticatorApp = _settings.TwoFactorAllowAuthenticatorApp,
            TwoFactorAllowCodeDelivery = _settings.TwoFactorAllowCodeDelivery,
            TwoFactorCodeDeliveryChannel = _settings.TwoFactorCodeDeliveryChannel,
            TwoFactorAllowEmailChannel = _settings.TwoFactorAllowEmailChannel,
            TwoFactorAllowSmsChannel = _settings.TwoFactorAllowSmsChannel,
            TwoFactorAllowVoiceChannel = _settings.TwoFactorAllowVoiceChannel,
            OtpIsEnabled = _settings.OtpIsEnabled,
            OtpAllowRegistration = _settings.OtpAllowRegistration,
            OtpAllowLogin = _settings.OtpAllowLogin,
            OtpDefaultChannel = _settings.OtpDefaultChannel,
            OtpAllowEmailChannel = _settings.OtpAllowEmailChannel,
            OtpAllowSmsChannel = _settings.OtpAllowSmsChannel,
            OtpAllowVoiceChannel = _settings.OtpAllowVoiceChannel,
            OtpMaxAttemptsPerCode = _settings.OtpMaxAttemptsPerCode,
            OtpRateLimitPerIdentifierPerHour = _settings.OtpRateLimitPerIdentifierPerHour,
            OtpResendCooldownSeconds = _settings.OtpResendCooldownSeconds,
            OtpResendBackoffAfterAttempts = _settings.OtpResendBackoffAfterAttempts,
            OtpResendBackoffMultiplier = _settings.OtpResendBackoffMultiplier,
            OtpResendMaxCooldownSeconds = _settings.OtpResendMaxCooldownSeconds,
            OtpResendCounterResetHours = _settings.OtpResendCounterResetHours,
            CaptchaIsEnabled = _settings.CaptchaIsEnabled,
            CaptchaProvider = _settings.CaptchaProvider,
            CaptchaRequiredOnRegister = _settings.CaptchaRequiredOnRegister,
            CaptchaRequiredOnForgotPassword = _settings.CaptchaRequiredOnForgotPassword,
            CaptchaRequiredOnOtpSend = _settings.CaptchaRequiredOnOtpSend,
            CaptchaRequiredOnLogin = _settings.CaptchaRequiredOnLogin,
            CaptchaRequiredOnEmailConfirmationResend = _settings.CaptchaRequiredOnEmailConfirmationResend,
            CaptchaChallengeExpirationMinutes = _settings.CaptchaChallengeExpirationMinutes,
            CaptchaTurnstileSiteKey = _settings.CaptchaTurnstileSiteKey,
            CaptchaTurnstileSecretKey = _settings.CaptchaTurnstileSecretKey,
            CaptchaRecaptchaSiteKey = _settings.CaptchaRecaptchaSiteKey,
            CaptchaRecaptchaSecretKey = _settings.CaptchaRecaptchaSecretKey,
            CaptchaRecaptchaVersion = _settings.CaptchaRecaptchaVersion
        };
}

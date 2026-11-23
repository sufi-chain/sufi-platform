using Shouldly;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.Settings;

public class IdentitySettingsRulesTests
{
    [Fact]
    public void Valid_defaults_are_accepted()
    {
        Should.NotThrow(() => IdentitySettingsRules.Validate(new IdentitySettingsDto()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(129)]
    public void Password_length_outside_6_to_128_is_rejected(int length)
    {
        var input = new IdentitySettingsDto { PasswordRequiredLength = length };

        var exception = Should.Throw<AbpValidationException>(() => IdentitySettingsRules.Validate(input));

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(input.PasswordRequiredLength)));
    }

    [Fact]
    public void Lockout_attempts_must_stay_between_1_and_100()
    {
        var input = new IdentitySettingsDto { LockoutMaxFailedAccessAttempts = 0 };

        var exception = Should.Throw<AbpValidationException>(() => IdentitySettingsRules.Validate(input));

        exception.ValidationErrors.ShouldContain(error =>
            error.MemberNames.Contains(nameof(input.LockoutMaxFailedAccessAttempts)));
    }

    [Fact]
    public void Turnstile_without_keys_is_rejected_and_a_stored_secret_can_stay_blank()
    {
        var missing = new IdentitySettingsDto { CaptchaProvider = "Turnstile" };

        var exception = Should.Throw<AbpValidationException>(() => IdentitySettingsRules.RequireExternalCaptchaKeys(
            missing,
            storedTurnstileSecret: null,
            storedRecaptchaSecret: null,
            "site",
            "secret",
            "rsite",
            "rsecret"));

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(missing.CaptchaTurnstileSiteKey)));
        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(missing.CaptchaTurnstileSecretKey)));

        var kept = new IdentitySettingsDto
        {
            CaptchaProvider = "Turnstile",
            CaptchaTurnstileSiteKey = "site-key"
        };
        Should.NotThrow(() => IdentitySettingsRules.RequireExternalCaptchaKeys(
            kept,
            storedTurnstileSecret: "already-stored",
            storedRecaptchaSecret: null,
            "site",
            "secret",
            "rsite",
            "rsecret"));
    }
}

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
}

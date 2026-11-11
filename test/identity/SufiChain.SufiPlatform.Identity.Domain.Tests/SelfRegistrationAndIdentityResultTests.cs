using Microsoft.AspNetCore.Identity;
using Shouldly;
using SufiChain.SufiPlatform.Identity.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class SelfRegistrationAndIdentityResultTests
{
    [Theory]
    [InlineData(null, null, true)]
    [InlineData("true", null, true)]
    [InlineData("false", null, false)]
    [InlineData("false", "true", false)]
    [InlineData("true", "false", false)]
    [InlineData("true", "true", true)]
    public void Self_registration_requires_both_settings_when_the_account_setting_is_present(
        string? sufi,
        string? abp,
        bool enabled)
    {
        SelfRegistrationPolicy.IsEnabled(sufi, abp).ShouldBe(enabled);
    }

    [Fact]
    public void Duplicate_username_is_a_business_exception_with_the_identity_code()
    {
        var result = IdentityResult.Failed(new IdentityError
        {
            Code = "DuplicateUserName",
            Description = "Username 'ada' is already taken."
        });

        var exception = Should.Throw<AbpIdentityResultException>(() => result.CheckErrors());

        exception.IdentityResult.Errors.ShouldContain(error => error.Code == "DuplicateUserName");
    }
}

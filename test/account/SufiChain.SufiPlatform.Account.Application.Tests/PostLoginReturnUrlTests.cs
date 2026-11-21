using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class PostLoginReturnUrlTests
{
    [Theory]
    [InlineData("/panel/portal/profile?tab=password")]
    [InlineData("/panel/portal/profile/?tab=password")]
    [InlineData("%2Fpanel%2Fportal%2Fprofile%3Ftab%3Dpassword")]
    [InlineData("/account/change-password")]
    [InlineData("/ar/panel/portal/profile?tab=password")]
    [InlineData("/fa/panel/portal/profile?tab=password")]
    [InlineData("/en/panel/portal/profile?tab=password")]
    [InlineData("/es/panel/portal/profile?tab=password")]
    public void Stale_password_tab_return_urls_are_rejected(string returnUrl)
    {
        PostLoginReturnUrl.IsStalePasswordTab(returnUrl).ShouldBeTrue();
        PostLoginReturnUrl.Resolve(returnUrl).ShouldBe(PostLoginReturnUrl.DashboardPath);
    }

    [Fact]
    public void Forced_password_change_still_lands_on_the_password_tab()
    {
        PostLoginReturnUrl.Resolve("/panel/dashboard", forcePasswordChange: true)
            .ShouldBe(PostLoginReturnUrl.ForcedPasswordChangePath);
    }

    [Fact]
    public void Forced_password_change_wins_over_a_stale_token_redirect()
    {
        PostLoginReturnUrl.Resolve("/panel/portal/profile?tab=password", forcePasswordChange: true)
            .ShouldBe(PostLoginReturnUrl.ForcedPasswordChangePath);
    }

    [Fact]
    public void Ordinary_panel_return_urls_are_kept_when_the_flag_is_false()
    {
        PostLoginReturnUrl.Resolve("/panel/admin/chat").ShouldBe("/panel/admin/chat");
        PostLoginReturnUrl.IsStalePasswordTab("/panel/portal/profile?tab=profile").ShouldBeFalse();
        PostLoginReturnUrl.Resolve("/panel/portal/profile?tab=password", forcePasswordChange: false)
            .ShouldBe(PostLoginReturnUrl.DashboardPath);
    }
}

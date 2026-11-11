using Shouldly;
using SufiChain.SufiPlatform.Account.Blazor.Pages;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class LoginFieldGapsTests
{
    [Fact]
    public void Empty_submit_marks_both_fields()
    {
        var gaps = LoginFieldGaps.From("  ", "");
        gaps.UserNameOrEmail.ShouldBeTrue();
        gaps.Password.ShouldBeTrue();
        gaps.Any.ShouldBeTrue();
    }

    [Fact]
    public void Username_only_does_not_ask_for_the_username_again()
    {
        var gaps = LoginFieldGaps.From("qa-user", null);
        gaps.UserNameOrEmail.ShouldBeFalse();
        gaps.Password.ShouldBeTrue();
    }

    [Fact]
    public void Both_fields_filled_has_no_gap()
    {
        LoginFieldGaps.From("qa-user", "secret").Any.ShouldBeFalse();
    }
}

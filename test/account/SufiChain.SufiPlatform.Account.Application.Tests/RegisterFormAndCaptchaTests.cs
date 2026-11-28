using SufiChain.SufiPlatform.Account;
using SufiChain.SufiPlatform.Captcha;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Application.Tests;

public class RegisterFormAndCaptchaTests
{
    [Fact]
    public void Register_InvalidEmail_ShowsFieldError()
    {
        RegisterFormCheck.Error("not-an-email", "secret", "secret").ShouldBe("InvalidEmail");
    }

    [Fact]
    public void Register_PasswordMismatch_ShowsFieldError()
    {
        RegisterFormCheck.Error("a@b.co", "secret", "other").ShouldBe("ConfirmPasswordMismatch");
    }

    [Fact]
    public void Register_PersianCaptchaDigits_AreAccepted()
    {
        CaptchaDigits.Normalize("۱۲").ShouldBe("12");
        CaptchaDigits.Normalize("١٢").ShouldBe("12");
        CaptchaDigits.Normalize("12").ShouldBe("12");
        CaptchaDigits.ToPersian("2 + 3 = ?").ShouldBe("۲ + ۳ = ?");
    }
}

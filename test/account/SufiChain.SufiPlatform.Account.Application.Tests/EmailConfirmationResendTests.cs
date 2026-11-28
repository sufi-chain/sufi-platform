using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class EmailConfirmationResendTests
{
    [Fact]
    public void EmailConfirmationResend_ReloadedCircuit_RequiresFreshCaptchaOrSucceedsWithoutStaleToken()
    {
        EmailConfirmationResend.NeedsFreshCaptcha(null).ShouldBeFalse();
        EmailConfirmationResend.NeedsFreshCaptcha(IdentitySecurityErrorCodes.CaptchaValidationFailed).ShouldBeTrue();
    }

    [Fact]
    public void EmailConfirmationResend_CaptchaValidationFailed_ShowsCaptchaSpecificMessage()
    {
        EmailConfirmationResend.MessageKey(IdentitySecurityErrorCodes.CaptchaValidationFailed)
            .ShouldBe(EmailConfirmationResend.CaptchaMessageKey);
        EmailConfirmationResend.CaptchaMessageKey.ShouldNotBe(EmailConfirmationResend.GenericMessageKey);
    }

    [Fact]
    public void EmailConfirmationResend_Success_StillWorksWithFreshCaptcha()
    {
        EmailConfirmationResend.MessageKey(null).ShouldBe(EmailConfirmationResend.GenericMessageKey);
        EmailConfirmationResend.NeedsFreshCaptcha("Sufi.Account:SomeOtherError").ShouldBeFalse();
    }
}

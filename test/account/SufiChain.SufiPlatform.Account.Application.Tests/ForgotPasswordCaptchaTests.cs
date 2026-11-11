using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Account.Blazor.Pages;
using SufiChain.SufiPlatform.Captcha;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class ForgotPasswordCaptchaTests
{
    [Fact]
    public async Task Wrong_captcha_is_rejected_before_the_address_is_looked_up()
    {
        var probe = new ResetProbe(Valid: false);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            probe.SendPasswordResetCodeAsync(Request("missing@example.com")));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        probe.Lookups.ShouldBe(0);
    }

    [Fact]
    public async Task Unknown_and_known_addresses_complete_the_same_way_when_captcha_passes()
    {
        var unknown = new ResetProbe(Valid: true);
        await unknown.SendPasswordResetCodeAsync(Request("missing@example.com"));
        unknown.Lookups.ShouldBe(1);

        var known = new ResetProbe(Valid: true)
        {
            Recipient = new IdentityUser(Guid.NewGuid(), "known", "known@example.com", null)
        };
        var exception = await Should.ThrowAsync<Exception>(() => known.SendPasswordResetCodeAsync(Request("known@example.com")));
        (exception is BusinessException business && business.Code == AccountErrorCodes.UserNotFoundWithEmail).ShouldBeFalse();
        known.Lookups.ShouldBe(1);
    }

    [Fact]
    public void Only_a_captcha_failure_uses_the_captcha_message()
    {
        ForgotPasswordFeedback.IsCaptchaFailure(
            new BusinessException(IdentitySecurityErrorCodes.CaptchaValidationFailed)).ShouldBeTrue();
        ForgotPasswordFeedback.IsCaptchaFailure(
            new BusinessException(AccountErrorCodes.UserNotFoundWithEmail)).ShouldBeFalse();
        ForgotPasswordFeedback.IsCaptchaFailure(new InvalidOperationException("mail")).ShouldBeFalse();
        ForgotPasswordFeedback.CaptchaErrorKey.ShouldBe("Sufi.Identity:CaptchaValidationFailed");
    }

    [Theory]
    [InlineData("en", "The security check failed or expired.")]
    [InlineData("fa", "بررسی امنیتی ناموفق بود یا منقضی شد.")]
    [InlineData("ar", "فشل فحص الأمان أو انتهت صلاحيته.")]
    [InlineData("es", "La comprobación de seguridad falló o expiró.")]
    public void Captcha_error_is_localized(string culture, string text)
    {
        var json = File.ReadAllText(IdentityJson(culture));
        json.ShouldContain("\"Sufi.Identity:CaptchaValidationFailed\"");
        json.ShouldContain(text);
    }

    private static SendPasswordResetCodeDto Request(string email)
    {
        return new SendPasswordResetCodeDto
        {
            Email = email,
            AppName = "tests",
            CaptchaChallengeId = "challenge",
            CaptchaAnswer = "0"
        };
    }

    private static string IdentityJson(string culture)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !Directory.Exists(Path.Combine(dir.FullName, "modules", "identity")))
        {
            dir = dir.Parent;
        }

        dir.ShouldNotBeNull();
        return Path.Combine(
            dir.FullName,
            "modules",
            "identity",
            "src",
            "SufiChain.SufiPlatform.Identity.Domain.Shared",
            "Localization",
            "Identity",
            culture + ".json");
    }

    private sealed class ResetProbe : AccountAppService
    {
        public int Lookups { get; private set; }

        public IdentityUser? Recipient { get; set; }

        public ResetProbe(bool Valid)
            : base(
                null!,
                null!,
                Options.Create(new IdentityOptions()),
                null!,
                null!,
                Captcha(Valid),
                null!,
                null!,
                null!,
                null!)
        {
        }

        protected override Task<IdentityUser?> FindPasswordResetRecipientAsync(string email)
        {
            Lookups++;
            return Task.FromResult(Recipient);
        }

        private static ICaptchaValidator Captcha(bool valid)
        {
            var captcha = Substitute.For<ICaptchaValidator>();
            captcha.ValidateAsync(Arg.Any<CaptchaValidationContext>(), Arg.Any<CancellationToken>())
                .Returns(valid
                    ? CaptchaValidationResult.Success()
                    : CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed));
            return captcha;
        }
    }
}

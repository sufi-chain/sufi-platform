using System.Diagnostics;
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
        unknown.Queued.ShouldBe(0);

        var known = new ResetProbe(Valid: true)
        {
            Recipient = new IdentityUser(Guid.NewGuid(), "known", "known@example.com", null)
        };
        await known.SendPasswordResetCodeAsync(Request("known@example.com"));
        known.Lookups.ShouldBe(1);
        known.Queued.ShouldBe(1);
        known.QueuedUserId.ShouldBe(known.Recipient.Id);
        known.QueuedEmail.ShouldBe("known@example.com");
    }

    [Fact]
    public async Task Known_and_unknown_addresses_both_wait_out_the_response_floor()
    {
        var floor = TimeSpan.FromMilliseconds(80);
        var unknown = new ResetProbe(Valid: true, floor);
        var unknownStarted = Stopwatch.GetTimestamp();
        await unknown.SendPasswordResetCodeAsync(Request("missing@example.com"));
        Stopwatch.GetElapsedTime(unknownStarted).ShouldBeGreaterThanOrEqualTo(floor);
        unknown.Queued.ShouldBe(0);

        var known = new ResetProbe(Valid: true, floor)
        {
            Recipient = new IdentityUser(Guid.NewGuid(), "known", "known@example.com", null)
        };
        var knownStarted = Stopwatch.GetTimestamp();
        await known.SendPasswordResetCodeAsync(Request("known@example.com"));
        Stopwatch.GetElapsedTime(knownStarted).ShouldBeGreaterThanOrEqualTo(floor);
        known.Queued.ShouldBe(1);
    }

    [Fact]
    public async Task A_queue_failure_for_a_known_address_still_returns_after_the_floor()
    {
        var floor = TimeSpan.FromMilliseconds(80);
        var known = new ResetProbe(Valid: true, floor)
        {
            Recipient = new IdentityUser(Guid.NewGuid(), "known", "known@example.com", null),
            FailQueue = true
        };
        var started = Stopwatch.GetTimestamp();
        await known.SendPasswordResetCodeAsync(Request("known@example.com"));
        Stopwatch.GetElapsedTime(started).ShouldBeGreaterThanOrEqualTo(floor);
        known.Lookups.ShouldBe(1);
        known.Queued.ShouldBe(0);
        known.QueueFailures.ShouldBe(1);
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

        public int Queued { get; private set; }

        public Guid QueuedUserId { get; private set; }

        public string? QueuedEmail { get; private set; }

        public IdentityUser? Recipient { get; set; }

        public bool FailQueue { get; set; }

        public int QueueFailures { get; private set; }

        private readonly TimeSpan? _floor;

        public ResetProbe(bool Valid, TimeSpan? floor = null)
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
            _floor = floor;
        }

        protected override TimeSpan PasswordResetResponseFloor => _floor ?? TimeSpan.Zero;

        protected override Task QueuePasswordResetDeliveryAsync(IdentityUser user, SendPasswordResetCodeDto input)
        {
            if (FailQueue)
            {
                throw new InvalidOperationException("queue down");
            }

            Queued++;
            QueuedUserId = user.Id;
            QueuedEmail = input.Email;
            return Task.CompletedTask;
        }

        protected override void LogPasswordResetQueueFailure(Exception exception, IdentityUser user)
        {
            QueueFailures++;
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

using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Captcha;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiCRM.Contacts.Onboarding;
using Volo.Abp;
using Volo.Abp.Caching;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCRM.Contacts;

public class OnboardingOtpCaptchaGateTests
{
    [Fact]
    public async Task Stored_Proof_Uses_The_Verified_Send_And_Is_Consumed_Once()
    {
        var proofs = new ProofFixture();
        await proofs.Store.StoreAsync("invite");
        var validator = Substitute.For<ICaptchaValidator>();
        var gate = new OnboardingOtpCaptchaGate(validator, proofs.Store);

        (await gate.UseVerifiedSendAsync(new SendOnboardingOtpInput { InvitationToken = "invite" })).ShouldBeTrue();
        (await proofs.Store.TryConsumeAsync("invite")).ShouldBeFalse();

        await validator.DidNotReceiveWithAnyArgs().ValidateAsync(default!, default);
    }

    [Fact]
    public async Task Captcha_Payload_Without_A_Proof_Lets_Account_Validate_It()
    {
        var validator = Substitute.For<ICaptchaValidator>();
        var gate = new OnboardingOtpCaptchaGate(validator, new ProofFixture().Store);

        var verified = await gate.UseVerifiedSendAsync(new SendOnboardingOtpInput
        {
            InvitationToken = "invite",
            CaptchaChallengeId = "challenge",
            CaptchaAnswer = "4"
        });

        verified.ShouldBeFalse();
        await validator.DidNotReceiveWithAnyArgs().ValidateAsync(default!, default);
    }

    [Fact]
    public async Task Disabled_Or_Optional_Captcha_Does_Not_Require_A_Proof()
    {
        var validator = Substitute.For<ICaptchaValidator>();
        validator.ValidateAsync(Arg.Any<CaptchaValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(CaptchaValidationResult.Success());
        var gate = new OnboardingOtpCaptchaGate(validator, new ProofFixture().Store);

        (await gate.UseVerifiedSendAsync(new SendOnboardingOtpInput { InvitationToken = "invite" })).ShouldBeFalse();
    }

    [Fact]
    public async Task Required_Captcha_Without_Proof_Or_Payload_Is_Rejected()
    {
        var validator = Substitute.For<ICaptchaValidator>();
        validator.ValidateAsync(Arg.Any<CaptchaValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(CaptchaValidationResult.Failure("required"));
        var gate = new OnboardingOtpCaptchaGate(validator, new ProofFixture().Store);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            gate.UseVerifiedSendAsync(new SendOnboardingOtpInput { InvitationToken = "invite" }));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.CaptchaValidationFailed);
    }

    [Fact]
    public async Task Blank_Invitation_Cannot_Consume_A_Proof()
    {
        var proofs = new ProofFixture();

        (await proofs.Store.TryConsumeAsync("  ")).ShouldBeFalse();
        (await proofs.Store.TryConsumeAsync(string.Empty)).ShouldBeFalse();
    }

    private sealed class ProofFixture
    {
        public ProofFixture()
        {
            var cache = Substitute.For<IDistributedCache<string>>();
            cache.GetAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call => Items.GetValueOrDefault(call.ArgAt<string>(0)));
            cache.SetAsync(
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<DistributedCacheEntryOptions?>(),
                    Arg.Any<bool?>(),
                    Arg.Any<bool>(),
                    Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    Items[call.ArgAt<string>(0)] = call.ArgAt<string>(1);
                    return Task.CompletedTask;
                });
            cache.RemoveAsync(Arg.Any<string>(), Arg.Any<bool?>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    Items.Remove(call.ArgAt<string>(0));
                    return Task.CompletedTask;
                });

            Store = new OnboardingOtpCaptchaProofStore(cache);
        }

        public OnboardingOtpCaptchaProofStore Store { get; }

        public Dictionary<string, string> Items { get; } = new();
    }
}

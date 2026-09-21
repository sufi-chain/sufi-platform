using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Captcha;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class CaptchaValidatorTests
{
    [Fact]
    public async Task Should_Succeed_When_Captcha_Is_Disabled()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.Captcha.IsEnabled).Returns(bool.FalseString);
        var resolver = Substitute.For<ICaptchaProviderResolver>();
        var validator = new CaptchaValidator(settings, resolver);

        var result = await validator.ValidateAsync(new CaptchaValidationContext
        {
            Purpose = CaptchaPurpose.Login,
            Token = "invalid"
        });

        result.IsValid.ShouldBeTrue();
        await resolver.DidNotReceive().ResolveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Succeed_When_Purpose_Is_Not_Required()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.Captcha.IsEnabled).Returns(bool.TrueString);
        settings.GetOrNullAsync(IdentitySettingNames.Captcha.RequiredOnLogin).Returns(bool.FalseString);
        var resolver = Substitute.For<ICaptchaProviderResolver>();
        var validator = new CaptchaValidator(settings, resolver);

        var result = await validator.ValidateAsync(new CaptchaValidationContext
        {
            Purpose = CaptchaPurpose.Login,
            Token = "invalid"
        });

        result.IsValid.ShouldBeTrue();
        await resolver.DidNotReceive().ResolveAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Delegate_To_Provider_When_Required()
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.Captcha.IsEnabled).Returns(bool.TrueString);
        settings.GetOrNullAsync(IdentitySettingNames.Captcha.RequiredOnRegister).Returns(bool.TrueString);
        var provider = Substitute.For<ICaptchaProvider>();
        provider.ValidateAsync(Arg.Any<CaptchaValidationContext>(), Arg.Any<CancellationToken>())
            .Returns(CaptchaValidationResult.Failure("bad-token"));
        var resolver = Substitute.For<ICaptchaProviderResolver>();
        resolver.ResolveAsync(Arg.Any<CancellationToken>()).Returns(provider);
        var validator = new CaptchaValidator(settings, resolver);

        var result = await validator.ValidateAsync(new CaptchaValidationContext
        {
            Purpose = CaptchaPurpose.Register,
            Token = "bad-token"
        });

        result.IsValid.ShouldBeFalse();
        await provider.Received(1).ValidateAsync(Arg.Any<CaptchaValidationContext>(), Arg.Any<CancellationToken>());
    }
}

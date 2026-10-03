using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Sms;
using SufiChain.SufiPlatform.TextTemplating;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Otp;

public class VerificationOtpSenderTests
{
    [Theory]
    [InlineData(VerificationPurpose.OtpLogin, OtpPurposes.Login)]
    [InlineData(VerificationPurpose.OtpRegistration, OtpPurposes.Registration)]
    [InlineData(VerificationPurpose.TwoFactorCode, OtpPurposes.TwoFactor)]
    [InlineData(VerificationPurpose.PhoneConfirmation, OtpPurposes.PhoneConfirmation)]
    public void ToOtpPurpose_Should_Map_Each_Account_Purpose(VerificationPurpose purpose, string expected)
    {
        SmsVerificationChannelSender.ToOtpPurpose(purpose).ShouldBe(expected);
    }

    [Fact]
    public void CreateOtpMessage_Should_Issue_A_New_Idempotency_Key_Per_Code()
    {
        var message = NewMessage();

        var first = SmsVerificationChannelSender.CreateOtpMessage(message, "Your code is 123456");
        var second = SmsVerificationChannelSender.CreateOtpMessage(message, "Your code is 123456");

        first.Phone.ShouldBe("+989121234567");
        first.Code.ShouldBe("123456");
        first.Purpose.ShouldBe(OtpPurposes.Login);
        first.Content.ShouldBe("Your code is 123456");
        first.IdempotencyKey.ShouldStartWith("OtpLogin:+989121234567:");
        first.IdempotencyKey.ShouldNotBe(second.IdempotencyKey);
    }

    [Fact]
    public async Task SendAsync_Should_Use_The_Otp_Route()
    {
        var smsSender = Substitute.For<ISmsSender>();
        var sender = new SmsVerificationChannelSender(smsSender, NewRenderer());

        await sender.SendAsync(NewMessage());

        await smsSender.Received(1).SendOtpAsync(Arg.Is<OtpMessage>(m =>
            m.Phone == "+989121234567" && m.Code == "123456" && m.Purpose == OtpPurposes.Login));
        await smsSender.DidNotReceiveWithAnyArgs().SendAsync(default!, default!);
        await smsSender.DidNotReceiveWithAnyArgs().QueueAsync(default!, default!);
    }

    [Fact]
    public async Task SendAsync_Should_Not_Reveal_Delivery_Failures()
    {
        var smsSender = Substitute.For<ISmsSender>();
        smsSender.SendOtpAsync(Arg.Any<OtpMessage>())
            .Returns(Task.FromException(new InvalidOperationException("gateway down")));
        var sender = new SmsVerificationChannelSender(smsSender, NewRenderer());

        await Should.NotThrowAsync(() => sender.SendAsync(NewMessage()));
    }

    [Theory]
    [InlineData(6, 6)]
    [InlineData(8, 8)]
    [InlineData(0, OtpCodeGenerator.DefaultLength)]
    [InlineData(3, OtpCodeGenerator.DefaultLength)]
    public void OtpCodeGenerator_Should_Return_Only_Digits(int length, int expectedLength)
    {
        var code = OtpCodeGenerator.Generate(length);

        code.Length.ShouldBe(expectedLength);
        code.ShouldAllBe(c => c >= '0' && c <= '9');
    }

    private static VerificationMessage NewMessage() => new()
    {
        Purpose = VerificationPurpose.OtpLogin,
        Channel = VerificationDeliveryChannel.Sms,
        Recipient = "+989121234567",
        Code = "123456",
        AppName = "Sufi"
    };

    private static ITemplateRenderer NewRenderer()
    {
        var renderer = Substitute.For<ITemplateRenderer>();
        renderer.RenderAsync(Arg.Any<string>(), Arg.Any<object?>(), Arg.Any<string?>(), Arg.Any<Dictionary<string, object>?>())
            .Returns("Your code is 123456");
        return renderer;
    }
}

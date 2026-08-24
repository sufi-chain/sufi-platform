using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Account.Localization;
using SufiChain.SufiPlatform.Account.Templates;
using SufiChain.SufiPlatform.SufiCom.Email;
using SufiChain.SufiPlatform.TextTemplating;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class EmailVerificationChannelSenderTests
{
    [Theory]
    [InlineData(VerificationPurpose.EmailConfirmation, AccountTemplates.EmailConfirmation, "EmailConfirmation:Subject")]
    [InlineData(VerificationPurpose.PasswordReset, AccountTemplates.PasswordReset, "PasswordReset:Subject")]
    [InlineData(VerificationPurpose.OtpLogin, AccountTemplates.VerificationCode, "VerificationCode:Subject")]
    [InlineData(VerificationPurpose.OtpRegistration, AccountTemplates.VerificationCode, "VerificationCode:Subject")]
    [InlineData(VerificationPurpose.TwoFactorCode, AccountTemplates.VerificationCode, "VerificationCode:Subject")]
    public async Task SendAsync_Queues_The_Purpose_Specific_Auth_Template(
        VerificationPurpose purpose,
        string expectedTemplate,
        string expectedSubjectKey)
    {
        var emailSender = Substitute.For<IEmailSender>();
        var templateRenderer = Substitute.For<ITemplateRenderer>();
        var localizer = CreateLocalizer();
        var capturedModel = default(object);

        templateRenderer
            .RenderAsync(expectedTemplate, Arg.Any<object?>(), Arg.Any<string?>(), Arg.Any<Dictionary<string, object>?>())
            .Returns(call =>
            {
                capturedModel = call.ArgAt<object?>(1);
                return Task.FromResult($"rendered:{expectedTemplate}");
            });

        var sender = new EmailVerificationChannelSender(emailSender, templateRenderer, localizer);

        await sender.SendAsync(new VerificationMessage
        {
            Purpose = purpose,
            Recipient = "user@example.com",
            Link = "https://app.example/confirm",
            Code = "123456",
            UserName = "pooria",
            AppName = "MVC"
        });

        await emailSender.Received(1).QueueAsync(
            "user@example.com",
            expectedSubjectKey,
            $"rendered:{expectedTemplate}",
            true,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<SufiChain.SufiPlatform.SufiCom.MessageAttachment>?>(),
            Arg.Any<SufiChain.SufiPlatform.SufiCom.AdditionalMessageSendingArgs?>());

        await emailSender.DidNotReceive().SendAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<SufiChain.SufiPlatform.SufiCom.MessageAttachment>?>(),
            Arg.Any<SufiChain.SufiPlatform.SufiCom.AdditionalMessageSendingArgs?>());

        capturedModel.ShouldNotBeNull();
        var modelType = capturedModel.GetType();
        modelType.GetProperty("link")!.GetValue(capturedModel).ShouldBe("https://app.example/confirm");
        modelType.GetProperty("code")!.GetValue(capturedModel).ShouldBe("123456");
        modelType.GetProperty("userName")!.GetValue(capturedModel).ShouldBe("pooria");
        modelType.GetProperty("appName")!.GetValue(capturedModel).ShouldBe("MVC");
    }

    private static IStringLocalizer<SufiAccountResource> CreateLocalizer()
    {
        var localizer = Substitute.For<IStringLocalizer<SufiAccountResource>>();
        localizer[Arg.Any<string>()].Returns(call =>
        {
            var name = call.Arg<string>();
            return new LocalizedString(name, name);
        });
        return localizer;
    }
}

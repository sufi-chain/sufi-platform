using NSubstitute;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Email;
using SufiChain.SufiPlatform.SufiCom.Smtp;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class DynamicEmailSenderTests
{
    [Fact]
    public async Task QueueAsync_Uses_NullSender_When_Smtp_Host_Is_Missing()
    {
        var smtp = Substitute.For<ISmtpEmailSender>();
        var sender = CreateSender(smtpHost: null, smtp);

        await sender.QueueAsync("user@example.com", "subject", "body");

        await smtp.DidNotReceive().QueueAsync(
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<SufiChain.SufiPlatform.SufiCom.MessageAttachment>?>(),
            Arg.Any<AdditionalMessageSendingArgs?>());
    }

    [Fact]
    public async Task QueueAsync_Uses_Smtp_When_Host_Is_Configured()
    {
        var smtp = Substitute.For<ISmtpEmailSender>();
        var sender = CreateSender(smtpHost: "smtp.example.com", smtp);

        await sender.QueueAsync("user@example.com", "subject", "body", true);

        await smtp.Received(1).QueueAsync(
            "user@example.com",
            "subject",
            "body",
            true,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<SufiChain.SufiPlatform.SufiCom.MessageAttachment>?>(),
            Arg.Any<AdditionalMessageSendingArgs?>());
    }

    [Fact]
    public async Task SendAsync_Uses_Smtp_When_Host_Is_Configured()
    {
        var smtp = Substitute.For<ISmtpEmailSender>();
        var sender = CreateSender(smtpHost: "smtp.example.com", smtp);

        await sender.SendAsync("user@example.com", "subject", "body", true);

        await smtp.Received(1).SendAsync(
            "user@example.com",
            "subject",
            "body",
            true,
            Arg.Any<string?>(),
            Arg.Any<string?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<string>?>(),
            Arg.Any<IEnumerable<SufiChain.SufiPlatform.SufiCom.MessageAttachment>?>(),
            Arg.Any<AdditionalMessageSendingArgs?>());
    }

    private static DynamicEmailSender CreateSender(string? smtpHost, ISmtpEmailSender smtp)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(SufiComSenderSettingNames.Email.SmtpHost).Returns(smtpHost);
        return new DynamicEmailSender(new NullEmailSender(), smtp, settings);
    }
}

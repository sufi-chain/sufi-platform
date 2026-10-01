using System.Net.Mail;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.BackgroundJobs;
using SufiChain.SufiPlatform.SufiCom.Email;
using Volo.Abp.BackgroundJobs;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class EmailSenderDisplayNameTests
{
    [Fact]
    public async Task SendAsync_Uses_The_Caller_Display_Name_On_The_Shared_Address()
    {
        var sender = CreateSender("SufiAPP");

        await sender.SendAsync(
            "user@example.com",
            "subject",
            "body",
            additionalArgs: new AdditionalMessageSendingArgs
            {
                FromDisplayName = "Account"
            });

        sender.CapturedAddress.ShouldBe("no-reply@sufichain.com");
        sender.CapturedDisplayName.ShouldBe("Account");
    }

    [Fact]
    public async Task SendAsync_Uses_The_Smtp_Display_Name_When_The_Caller_Does_Not_Set_One()
    {
        var sender = CreateSender("SufiAPP");

        await sender.SendAsync("user@example.com", "subject", "body");

        sender.CapturedAddress.ShouldBe("no-reply@sufichain.com");
        sender.CapturedDisplayName.ShouldBe("SufiAPP");
    }

    [Fact]
    public async Task QueueAsync_Keeps_The_Caller_Display_Name_For_The_Background_Send()
    {
        var jobs = Substitute.For<IBackgroundJobManager>();
        BackgroundEmailSendingJobArgs? queued = null;
        jobs.EnqueueAsync(
                Arg.Do<BackgroundEmailSendingJobArgs>(args => queued = args),
                Arg.Any<BackgroundJobPriority>(),
                Arg.Any<TimeSpan?>())
            .Returns("job-id");

        var sender = CreateSender("SufiAPP", jobs);

        await sender.QueueAsync(
            "user@example.com",
            "subject",
            "body",
            additionalArgs: new AdditionalMessageSendingArgs
            {
                FromDisplayName = "Calendar"
            });

        queued.ShouldNotBeNull();
        queued.From.ShouldBe("no-reply@sufichain.com");
        queued.FromDisplayName.ShouldBe("Calendar");
    }

    private static CapturingEmailSender CreateSender(string defaultDisplayName, IBackgroundJobManager? jobs = null)
    {
        var configuration = Substitute.For<IEmailSenderConfiguration>();
        configuration.GetDefaultFromAddressAsync().Returns("no-reply@sufichain.com");
        configuration.GetDefaultFromDisplayNameAsync().Returns(defaultDisplayName);

        return new CapturingEmailSender(configuration, jobs ?? Substitute.For<IBackgroundJobManager>());
    }

    private sealed class CapturingEmailSender : EmailSenderBase
    {
        public CapturingEmailSender(
            IEmailSenderConfiguration configuration,
            IBackgroundJobManager backgroundJobManager)
            : base(configuration, backgroundJobManager)
        {
        }

        public string? CapturedAddress { get; private set; }

        public string? CapturedDisplayName { get; private set; }

        protected override Task SendEmailAsync(MailMessage mail)
        {
            CapturedAddress = mail.From?.Address;
            CapturedDisplayName = mail.From?.DisplayName;
            return Task.CompletedTask;
        }
    }
}

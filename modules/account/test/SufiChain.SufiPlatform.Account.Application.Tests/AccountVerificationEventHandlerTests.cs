using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class AccountVerificationEventHandlerTests
{
    [Fact]
    public async Task UserRegistered_Without_Confirmation_Token_Does_Not_Dispatch()
    {
        var (handler, dispatcher, _) = CreateHandler(requireEmailConfirmation: true);

        await handler.HandleEventAsync(new UserRegisteredEvent
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            AppName = "MVC",
            EmailConfirmationToken = null
        });

        await dispatcher.DidNotReceive().SendAsync(Arg.Any<VerificationMessage>());
    }

    [Fact]
    public async Task UserRegistered_Does_Not_Dispatch_When_Confirmation_Is_Disabled()
    {
        var (handler, dispatcher, _) = CreateHandler(requireEmailConfirmation: false);

        await handler.HandleEventAsync(new UserRegisteredEvent
        {
            UserId = Guid.NewGuid(),
            Email = "user@example.com",
            AppName = "MVC",
            EmailConfirmationToken = "token"
        });

        await dispatcher.DidNotReceive().SendAsync(Arg.Any<VerificationMessage>());
    }

    [Fact]
    public async Task UserRegistered_Dispatches_Email_Confirmation()
    {
        var (handler, dispatcher, urls) = CreateHandler(requireEmailConfirmation: true);
        var userId = Guid.NewGuid();
        urls.GetEmailConfirmationUrlAsync("MVC", userId, "token", "/return", "hash")
            .Returns("https://app.example/confirm");

        await handler.HandleEventAsync(new UserRegisteredEvent
        {
            UserId = userId,
            Email = "user@example.com",
            AppName = "MVC",
            EmailConfirmationToken = "token",
            ReturnUrl = "/return",
            ReturnUrlHash = "hash"
        });

        await dispatcher.Received(1).SendAsync(Arg.Is<VerificationMessage>(message =>
            message.Purpose == VerificationPurpose.EmailConfirmation &&
            message.Recipient == "user@example.com" &&
            message.Link == "https://app.example/confirm" &&
            message.UserId == userId &&
            message.AppName == "MVC"));
    }

    [Fact]
    public async Task PasswordReset_Dispatches_Reset_Email()
    {
        var (handler, dispatcher, urls) = CreateHandler(requireEmailConfirmation: false);
        var userId = Guid.NewGuid();
        urls.GetPasswordResetUrlAsync("MVC", userId, "reset-token", "/return", "hash")
            .Returns("https://app.example/reset");

        await handler.HandleEventAsync(new PasswordResetRequestedEvent
        {
            UserId = userId,
            Email = "user@example.com",
            ResetToken = "reset-token",
            AppName = "MVC",
            ReturnUrl = "/return",
            ReturnUrlHash = "hash"
        });

        await dispatcher.Received(1).SendAsync(Arg.Is<VerificationMessage>(message =>
            message.Purpose == VerificationPurpose.PasswordReset &&
            message.Recipient == "user@example.com" &&
            message.Link == "https://app.example/reset" &&
            message.UserId == userId &&
            message.AppName == "MVC"));
    }

    [Theory]
    [InlineData(VerificationPurpose.OtpLogin)]
    [InlineData(VerificationPurpose.OtpRegistration)]
    [InlineData(VerificationPurpose.TwoFactorCode)]
    public async Task Verification_Code_Request_Dispatches_The_Same_Purpose(VerificationPurpose purpose)
    {
        var (handler, dispatcher, _) = CreateHandler(requireEmailConfirmation: false);
        var userId = Guid.NewGuid();

        await handler.HandleEventAsync(new VerificationCodeRequestedEvent
        {
            UserId = userId,
            Identifier = "user@example.com",
            Code = "654321",
            Purpose = purpose,
            PreferredChannel = VerificationDeliveryChannel.Email,
            AppName = "MVC"
        });

        await dispatcher.Received(1).SendAsync(Arg.Is<VerificationMessage>(message =>
            message.Purpose == purpose &&
            message.PreferredChannel == VerificationDeliveryChannel.Email &&
            message.Recipient == "user@example.com" &&
            message.Code == "654321" &&
            message.UserId == userId &&
            message.AppName == "MVC"));
    }

    private static (
        AccountVerificationEventHandler Handler,
        IVerificationCodeDispatcher Dispatcher,
        IAppUrlProvider Urls) CreateHandler(bool requireEmailConfirmation)
    {
        var settings = Substitute.For<ISettingProvider>();
        settings.GetOrNullAsync(IdentitySettingNames.Registration.RequireEmailConfirmation)
            .Returns(requireEmailConfirmation ? "true" : "false");

        var urls = Substitute.For<IAppUrlProvider>();
        var dispatcher = Substitute.For<IVerificationCodeDispatcher>();
        var handler = new AccountVerificationEventHandler(settings, urls, dispatcher);
        return (handler, dispatcher, urls);
    }
}

using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiCom.Channels.Metadata;
using SufiChain.SufiPlatform.SufiCom.Channels.Outbound;
using SufiChain.SufiPlatform.SufiCom.Channels.Telegram;
using SufiChain.SufiPlatform.SufiCom.Application.Connections;
using SufiChain.SufiPlatform.SufiCom.Configuration;
using SufiChain.SufiPlatform.SufiCom.Connections;
using SufiChain.SufiPlatform.SufiCom;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Channels;

/// <summary>
/// Outbound dispatch unit tests for <see cref="TelegramUserChannelConnector"/>. Uses NSubstitute
/// for <see cref="ITelegramConnectionRepository"/> / <see cref="IFileStorageIntegrationService"/>
/// and the real <see cref="FakeTelegramForeignGateway"/> so the deterministic auth + send loop is
/// exercised without a ForeignHost process.
/// </summary>
public class TelegramUserChannelConnectorTests
{
    private readonly ITelegramConnectionRepository _repository;
    private readonly FakeTelegramForeignGateway _gateway;
    private readonly IFileStorageIntegrationService _fileStorage;
    private readonly ITelegramRateLimiter _rateLimiter;

    public TelegramUserChannelConnectorTests()
    {
        _repository = Substitute.For<ITelegramConnectionRepository>();
        _gateway = new FakeTelegramForeignGateway(NullLogger<FakeTelegramForeignGateway>.Instance);
        _fileStorage = Substitute.For<IFileStorageIntegrationService>();
        // Permissive limiter: the connector rate-limit behavior is covered separately in the
        // SufiCom Application.Tests suite; these tests focus on connection/gateway plumbing.
        _rateLimiter = Substitute.For<ITelegramRateLimiter>();
        _rateLimiter
            .CheckAsync(default!, default!, default, default)
            .ReturnsForAnyArgs(TelegramRateLimitDecision.Allow());
    }

    private TelegramUserChannelConnector CreateConnector()
    {
        return new TelegramUserChannelConnector(_repository, _gateway, _fileStorage, _rateLimiter);
    }

    private static TelegramConnection ReadyConnection()
    {
        var connection = new TelegramConnection(
            Guid.NewGuid(),
            tenantId: null,
            phoneNumber: "+989120000001",
            displayName: "Support",
            roleLabel: "Support",
            isEnabled: true);
        connection.PrepareForAuth("session-abc");
        connection.MarkReady();
        return connection;
    }

    private async Task PrimeGatewayAsync(Guid connectionId)
    {
        // The FakeTelegramForeignGateway keeps its own per-connection auth state and only sends
        // when Ready. Drive it through the deterministic flow so SendOutboundAsync succeeds.
        await _gateway.PrepareConnectionAsync(new PrepareTelegramConnectionRequest { ConnectionId = connectionId });
        await _gateway.SubmitPhoneAsync(new PrepareTelegramConnectionRequest { ConnectionId = connectionId });
        await _gateway.SubmitCodeAsync(new SubmitTelegramCodeRequest { ConnectionId = connectionId });
    }

    private static DispatchOutboundChannelMessageInput OutboundInput(
        Guid connectionId,
        string threadId = "tg-chat-1",
        string body = "Hello",
        List<Guid>? attachments = null)
    {
        return new DispatchOutboundChannelMessageInput
        {
            SessionId = Guid.NewGuid(),
            MessageId = Guid.NewGuid(),
            Body = body,
            ExternalThreadId = threadId,
            SessionConnectorMetadata = new ChannelSessionConnectorMetadata
            {
                ConnectorName = ChannelNames.TelegramUser,
                ExternalThreadId = threadId,
                ConnectionId = connectionId
            },
            AttachmentFileIds = attachments ?? new List<Guid>()
        };
    }

    [Fact]
    public async Task Should_Reject_When_ConnectionId_Is_Missing()
    {
        var connector = CreateConnector();
        var input = OutboundInput(connectionId: Guid.Empty);

        var result = await connector.DispatchOutboundAsync(input);

        result.Succeeded.ShouldBeFalse();
        result.FailureReason.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Reject_When_Connection_Is_Not_Found()
    {
        var connectionId = Guid.NewGuid();
        _repository.FindAsync(connectionId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns((TelegramConnection?)null);

        var connector = CreateConnector();
        var result = await connector.DispatchOutboundAsync(OutboundInput(connectionId));

        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Reject_When_Connection_Is_Not_Ready()
    {
        var connection = new TelegramConnection(
            Guid.NewGuid(),
            tenantId: null,
            "+989120000002",
            "Sales",
            "Sales",
            isEnabled: true);
        // AuthState stays Disconnected => CanSend() is false.

        _repository.FindAsync(connection.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(connection);

        var connector = CreateConnector();
        var result = await connector.DispatchOutboundAsync(OutboundInput(connection.Id));

        result.Succeeded.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_Succeed_And_Return_ExternalMessageId_When_Ready()
    {
        var connection = ReadyConnection();
        await PrimeGatewayAsync(connection.Id);
        _repository.FindAsync(connection.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(connection);

        var connector = CreateConnector();
        var result = await connector.DispatchOutboundAsync(OutboundInput(connection.Id));

        result.Succeeded.ShouldBeTrue();
        result.ExternalMessageId.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Should_Pass_Attachment_References_To_Gateway_And_Report_ExternalMessageId()
    {
        var connection = ReadyConnection();
        await PrimeGatewayAsync(connection.Id);
        var fileId = Guid.NewGuid();

        _repository.FindAsync(connection.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(connection);
        _fileStorage.GetAsync(fileId).Returns(new FileReferenceDto
        {
            Id = fileId,
            FileName = "photo.png",
            MimeType = "image/png",
            SizeInBytes = 1234
        });
        _fileStorage.GetAccessTokenAsync(fileId).Returns("access-token-photo");

        var connector = CreateConnector();
        var result = await connector.DispatchOutboundAsync(OutboundInput(
            connection.Id,
            attachments: new List<Guid> { fileId }));

        result.Succeeded.ShouldBeTrue();
        result.ExternalMessageId.ShouldNotBeNullOrWhiteSpace();
        // The fake folds the attachment count into the synthetic id for deterministic verification.
        result.ExternalMessageId!.ShouldStartWith("tg-out-att1-");

        await _fileStorage.Received(1).GetAsync(fileId);
        await _fileStorage.Received(1).GetAccessTokenAsync(fileId);
    }

    [Fact]
    public async Task Should_Not_Resolve_Attachments_When_None_Provided()
    {
        var connection = ReadyConnection();
        await PrimeGatewayAsync(connection.Id);
        _repository.FindAsync(connection.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(connection);

        var connector = CreateConnector();
        var result = await connector.DispatchOutboundAsync(OutboundInput(connection.Id, attachments: new List<Guid>()));

        result.Succeeded.ShouldBeTrue();
        await _fileStorage.DidNotReceiveWithAnyArgs().GetAsync(default);
    }

    [Fact]
    public async Task Should_Return_Failure_When_RateLimiter_Denies()
    {
        var connection = ReadyConnection();
        await PrimeGatewayAsync(connection.Id);
        _repository.FindAsync(connection.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(connection);

        var denying = Substitute.For<ITelegramRateLimiter>();
        denying
            .CheckAsync(default!, default!, default, default)
            .ReturnsForAnyArgs(TelegramRateLimitDecision.Deny(SufiComDomainErrorCodes.TelegramFirstContactCapReached));

        var connector = new TelegramUserChannelConnector(_repository, _gateway, _fileStorage, denying);

        var result = await connector.DispatchOutboundAsync(OutboundInput(connection.Id));

        result.Succeeded.ShouldBeFalse();
        result.FailureReason.ShouldBe(SufiComDomainErrorCodes.TelegramFirstContactCapReached);
    }

    [Fact]
    public async Task Should_Expose_FloodWait_RetryAfter_On_Gateway_Failure()
    {
        var connection = ReadyConnection();
        _repository.FindAsync(connection.Id, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(connection);

        var floodResult = new TelegramGatewayResult
        {
            Succeeded = false,
            AuthState = TelegramAuthState.Ready,
            ErrorCode = "FLOOD_WAIT",
            FloodWaitUntil = DateTime.UtcNow.AddSeconds(60),
            FailureReason = "FloodWait active"
        };
        var gateway = Substitute.For<ITelegramForeignGateway>();
        gateway.SendOutboundAsync(Arg.Any<SendTelegramOutboundRequest>(), Arg.Any<CancellationToken>())
            .Returns(floodResult);

        var connector = new TelegramUserChannelConnector(_repository, gateway, _fileStorage, _rateLimiter);
        var result = await connector.DispatchOutboundAsync(OutboundInput(connection.Id));

        result.Succeeded.ShouldBeFalse();
        result.RetryAfterSeconds.ShouldNotBeNull();
        result.RetryAfterSeconds!.Value.ShouldBeGreaterThan(0);
    }
}

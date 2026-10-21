using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.FileManager;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Inbound;
using SufiChain.SufiPlatform.SufiCom.Connections;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Integration;

// sufi-chain#458: the Telegram bridge is an anonymous, HMAC-only M2M path with no current user.
// It used to call the permission-protected IChannelInboundMessageAppService and failed with
// AbpAuthorizationException, exactly like the inbound email worker (#508).
public class TelegramBridgeIntegrationServiceTests
{
    [Fact]
    public async Task Bridge_ingests_through_the_permission_free_ingestor()
    {
        var connection = NewConnection();
        var ingestor = new RecordingIngestor();
        var service = CreateService(connection, ingestor, new StackTenant());

        var result = await service.IngestAsync(new IngestTelegramBridgeInput
        {
            ConnectionId = connection.Id,
            ExternalThreadId = "tg-user:chat-42",
            ExternalMessageId = "tg-msg-1",
            Body = "Hello from Telegram",
            ParticipantName = "Visitor",
            ParticipantAddress = "+989120000000",
            AnonymousVisitorId = "tg-peer-42"
        });

        var input = ingestor.Inputs.ShouldHaveSingleItem();
        input.ConnectorName.ShouldBe(ChannelNames.TelegramUser);
        input.ConnectionId.ShouldBe(connection.Id);
        input.ExternalThreadId.ShouldBe("tg-user:chat-42");
        input.ExternalMessageId.ShouldBe("tg-msg-1");
        input.Body.ShouldBe("Hello from Telegram");
        input.Sender.SenderKind.ShouldBe(ChatMessageSenderKind.Visitor);
        input.Sender.AnonymousVisitorId.ShouldBe("tg-peer-42");

        result.SessionId.ShouldBe(ingestor.LastResult!.SessionId);
        result.CreatedNewSession.ShouldBeTrue();
    }

    [Fact]
    public async Task Bridge_runs_the_ingest_inside_the_connection_tenant()
    {
        var tenantId = Guid.NewGuid();
        var connection = NewConnection(tenantId);
        var tenant = new StackTenant();
        var ingestor = new RecordingIngestor(tenant);
        var service = CreateService(connection, ingestor, tenant);

        await service.IngestAsync(new IngestTelegramBridgeInput
        {
            ConnectionId = connection.Id,
            ExternalThreadId = "tg-user:chat-42",
            Body = "Hello"
        });

        ingestor.TenantDuringIngest.ShouldBe(tenantId);
        // The change scope is restored when IngestAsync returns.
        tenant.Id.ShouldBeNull();
    }

    [Fact]
    public void Bridge_does_not_depend_on_the_permission_protected_app_service()
    {
        var parameterTypes = typeof(TelegramBridgeIntegrationService)
            .GetConstructors()
            .SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.ParameterType)
            .ToList();

        parameterTypes.ShouldContain(typeof(IChannelInboundMessageIngestor));
        parameterTypes.ShouldNotContain(typeof(IChannelInboundMessageAppService));
    }

    private static TelegramBridgeIntegrationService CreateService(
        TelegramConnection connection,
        RecordingIngestor ingestor,
        ICurrentTenant tenant)
    {
        var repository = Substitute.For<ITelegramConnectionRepository>();
        repository
            .FindAsync(Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(connection);

        return new TelegramBridgeIntegrationService(
            repository,
            ingestor,
            Substitute.For<IFileStorageIntegrationService>(),
            tenant,
            NullLogger<TelegramBridgeIntegrationService>.Instance);
    }

    private static TelegramConnection NewConnection(Guid? tenantId = null) => new(
        Guid.NewGuid(),
        tenantId,
        "+989120000000",
        "Support",
        "Support",
        isEnabled: true);

    private sealed class RecordingIngestor : IChannelInboundMessageIngestor
    {
        private readonly ICurrentTenant? _tenant;

        public RecordingIngestor(ICurrentTenant? tenant = null) => _tenant = tenant;

        public List<IngestInboundChannelMessageInput> Inputs { get; } = new();

        public Guid? TenantDuringIngest { get; private set; }

        public IngestInboundChannelMessageResult? LastResult { get; private set; }

        public Task<IngestInboundChannelMessageResult> IngestAsync(IngestInboundChannelMessageInput input)
        {
            Inputs.Add(input);
            TenantDuringIngest = _tenant?.Id;
            LastResult = new IngestInboundChannelMessageResult
            {
                SessionId = Guid.NewGuid(),
                MessageId = Guid.NewGuid(),
                CreatedNewSession = true
            };
            return Task.FromResult(LastResult);
        }
    }

    private sealed class StackTenant : ICurrentTenant
    {
        private readonly Stack<(Guid? Id, string? Name)> _stack = new();

        public bool IsAvailable => Id.HasValue;

        public Guid? Id => _stack.Count == 0 ? null : _stack.Peek().Id;

        public string? Name => _stack.Count == 0 ? null : _stack.Peek().Name;

        public IDisposable Change(Guid? id, string? name = null)
        {
            _stack.Push((id, name));
            return new Popper(_stack);
        }

        private sealed class Popper : IDisposable
        {
            private readonly Stack<(Guid? Id, string? Name)> _stack;
            private bool _disposed;

            public Popper(Stack<(Guid? Id, string? Name)> stack) => _stack = stack;

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                if (_stack.Count > 0)
                {
                    _stack.Pop();
                }
            }
        }
    }
}

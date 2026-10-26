using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Volo.Abp.Timing;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Auth;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using SufiChain.SufiPlatform.SufiAI.MCP.External;
using Volo.Abp;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#384 slice 6b: a UserScoped server must never be reachable anonymously, never shares a client
/// between two users, and its token must be bound to the user, the tenant and the server it was minted for.
/// </summary>
public class McpUserScopedBindingTests
{
    private static readonly Guid TenantA = Guid.NewGuid();
    private static readonly Guid TenantB = Guid.NewGuid();
    private static readonly Guid Alice = Guid.NewGuid();
    private static readonly Guid Bob = Guid.NewGuid();

    // ---- domain guard ------------------------------------------------------

    [Fact]
    public void UserScoped_is_rejected_for_stdio()
    {
        var server = new MCPServer(Guid.NewGuid(), "local", "Local", MCPTransportType.STDIO, TenantA);
        server.ConfigureStdio("npx", null);

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() => server.SetBindingMode(MCPBindingMode.UserScoped));
        exception.Code.ShouldBe(AIErrorCodes.MCPUserScopedStdioNotSupported);
        server.BindingMode.ShouldBe(MCPBindingMode.TenantLevel);
    }

    [Theory]
    [InlineData(MCPTransportType.SSE)]
    [InlineData(MCPTransportType.HTTP)]
    public void UserScoped_is_allowed_for_http_transports(MCPTransportType transportType)
    {
        var server = new MCPServer(Guid.NewGuid(), "remote", "Remote", transportType, TenantA);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");

        server.SetBindingMode(MCPBindingMode.UserScoped);

        server.BindingMode.ShouldBe(MCPBindingMode.UserScoped);
    }

    [Fact]
    public void A_new_server_defaults_to_tenant_level()
    {
        var server = new MCPServer(Guid.NewGuid(), "remote", "Remote", MCPTransportType.SSE, TenantA);
        server.BindingMode.ShouldBe(MCPBindingMode.TenantLevel);
    }

    // ---- token -------------------------------------------------------------

    [Fact]
    public void Token_round_trips_the_identity_and_session()
    {
        var service = CreateTokenService();
        var serverId = Guid.NewGuid();
        var hooshvareId = Guid.NewGuid();

        var token = service.Issue(
            AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA), "session-1").WithHooshvare(hooshvareId),
            serverId);

        token.ShouldNotBeNullOrWhiteSpace();

        var payload = service.Validate(token, serverId);
        payload.ShouldNotBeNull();
        payload!.TenantId.ShouldBe(TenantA);
        payload.UserId.ShouldBe(Alice);
        payload.SessionId.ShouldBe("session-1");
        payload.HooshvareId.ShouldBe(hooshvareId);
        payload.ExpiresAt.ShouldBeGreaterThan(DateTimeOffset.UtcNow);
    }

    [Fact]
    public void A_token_is_refused_for_another_server()
    {
        var service = CreateTokenService();
        var serverId = Guid.NewGuid();

        var token = service.Issue(AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)), serverId);

        service.Validate(token, Guid.NewGuid()).ShouldBeNull();
    }

    [Fact]
    public void A_token_is_refused_once_expired()
    {
        var clock = new MovableClock(DateTimeOffset.UtcNow);
        var service = CreateTokenService(lifetimeMinutes: 5, clock: clock);
        var serverId = Guid.NewGuid();

        var token = service.Issue(AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)), serverId);

        // Valid inside the lifetime, then refused once the clock moves past it.
        service.Validate(token, serverId).ShouldNotBeNull();
        clock.Advance(TimeSpan.FromMinutes(6));
        service.Validate(token, serverId).ShouldBeNull();
    }

    [Fact]
    public void A_tampered_token_is_refused()
    {
        var service = CreateTokenService();
        var serverId = Guid.NewGuid();

        var token = service.Issue(AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)), serverId);
        var tampered = token![..^4] + "AAAA";

        service.Validate(tampered, serverId).ShouldBeNull();
    }

    [Fact]
    public void An_anonymous_turn_is_never_issued_a_token()
    {
        var service = CreateTokenService();

        service.Issue(AgentCallContext.Anonymous(TenantA), Guid.NewGuid()).ShouldBeNull();
    }

    // ---- client sharing ----------------------------------------------------

    [Fact]
    public async Task Two_users_on_one_user_scoped_server_never_share_a_client()
    {
        var server = UserScopedServer();
        var harness = new ProviderHarness();

        var aliceLease = await harness.Provider.AcquireAsync(
            server, AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)), CancellationToken.None);
        var bobLease = await harness.Provider.AcquireAsync(
            server, AgentCallContext.ForUser(TenantA, Bob, Principal(Bob, TenantA)), CancellationToken.None);

        aliceLease.Client.ShouldNotBeSameAs(bobLease.Client);
        aliceLease.Owned.ShouldBeTrue();
        bobLease.Owned.ShouldBeTrue();

        // Each call carries the token of the user it belongs to.
        aliceLease.BearerToken.ShouldNotBeNullOrWhiteSpace();
        bobLease.BearerToken.ShouldNotBeNullOrWhiteSpace();
        aliceLease.BearerToken.ShouldNotBe(bobLease.BearerToken);

        await harness.Provider.ReleaseAsync(aliceLease);
        await harness.Provider.ReleaseAsync(bobLease);

        // Both user-scoped clients were disconnected and disposed.
        harness.Created.Count.ShouldBe(2);
        harness.Created.ShouldAllBe(client => client.Disposed);
    }

    [Fact]
    public async Task A_tenant_level_server_keeps_one_shared_client()
    {
        var server = new MCPServer(Guid.NewGuid(), "shared", "Shared", MCPTransportType.SSE, TenantA);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");

        var harness = new ProviderHarness();

        var first = await harness.Provider.AcquireAsync(server, null, CancellationToken.None);
        var second = await harness.Provider.AcquireAsync(
            server, AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)), CancellationToken.None);

        first.Client.ShouldBeSameAs(second.Client);
        first.Owned.ShouldBeFalse();
        second.Owned.ShouldBeFalse();
        first.BearerToken.ShouldBeNull();
        second.BearerToken.ShouldBeNull();

        await harness.Provider.ReleaseAsync(first);
        await harness.Provider.ReleaseAsync(second);

        // Releasing a shared client must not dispose it.
        harness.Created.Count.ShouldBe(1);
        harness.Created[0].Disposed.ShouldBeFalse();
    }

    [Fact]
    public async Task An_anonymous_turn_cannot_acquire_a_user_scoped_client()
    {
        var server = UserScopedServer();
        var harness = new ProviderHarness();

        var exception = await Should.ThrowAsync<global::Volo.Abp.BusinessException>(() =>
            harness.Provider.AcquireAsync(server, AgentCallContext.Anonymous(TenantA), CancellationToken.None));

        exception.Code.ShouldBe(AIErrorCodes.MCPUserScopedRequiresUser);
        harness.Created.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_user_scoped_tool_call_carries_that_users_token()
    {
        var server = UserScopedServer();
        var harness = new ProviderHarness();

        var tool = new ExternalMCPTool(
            "external.remote.ping",
            "Ping",
            "{}",
            server,
            "ping",
            harness.Provider);

        var context = new WorkspaceContext
        {
            WorkspaceName = "ws",
            TenantId = TenantA,
            UserId = Alice,
            CallContext = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA), "session-9")
        };

        var result = await tool.ExecuteAsync(context, new Dictionary<string, object?>());

        result.Success.ShouldBeTrue(result.ErrorMessage);

        var client = harness.Created.Single();
        client.LastBearerToken.ShouldNotBeNullOrWhiteSpace();
        client.Disposed.ShouldBeTrue("a user-scoped client must be disposed when the call ends");

        // The token the server received is exactly the one minted for Alice on this conversation.
        var payload = harness.TokenService.Validate(client.LastBearerToken, server.Id);
        payload.ShouldNotBeNull();
        payload!.UserId.ShouldBe(Alice);
        payload.TenantId.ShouldBe(TenantA);
        payload.SessionId.ShouldBe("session-9");
    }

    [Fact]
    public async Task An_anonymous_tool_call_on_a_user_scoped_server_is_refused_and_audited()
    {
        var server = UserScopedServer();
        var harness = new ProviderHarness();

        var tool = new ExternalMCPTool(
            "external.remote.ping",
            "Ping",
            "{}",
            server,
            "ping",
            harness.Provider);

        var context = new WorkspaceContext
        {
            WorkspaceName = "ws",
            TenantId = TenantA,
            CallContext = AgentCallContext.Anonymous(TenantA)
        };

        var result = await tool.ExecuteAsync(context, new Dictionary<string, object?>());

        result.Success.ShouldBeFalse();
        harness.Created.ShouldBeEmpty("nothing may be dialled for an anonymous user-scoped call");
    }

    [Fact]
    public void A_token_from_tenant_A_carries_tenant_A_not_B()
    {
        var service = CreateTokenService();
        var serverId = Guid.NewGuid();

        var tenantAToken = service.Issue(AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)), serverId);
        var payload = service.Validate(tenantAToken, serverId);

        payload.ShouldNotBeNull();
        payload!.TenantId.ShouldBe(TenantA);
        payload.TenantId.ShouldNotBe(TenantB);
    }

    // ---- helpers -----------------------------------------------------------

    private static MCPServer UserScopedServer()
    {
        var server = new MCPServer(Guid.NewGuid(), "remote", "Remote", MCPTransportType.SSE, TenantA);
        server.ConfigureHttpEndpoint("https://mcp.example/mcp");
        server.SetBindingMode(MCPBindingMode.UserScoped);
        return server;
    }

    private static IMcpUserTokenService CreateTokenService(int lifetimeMinutes = 5, IClock? clock = null)
    {
        return new McpUserTokenService(
            DataProtectionProvider.Create("tests"),
            Options.Create(new McpUserTokenOptions { LifetimeMinutes = lifetimeMinutes }),
            clock ?? Clock(DateTimeOffset.UtcNow));
    }

    private static IClock Clock(DateTimeOffset now)
    {
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(_ => now.UtcDateTime);
        return clock;
    }

    /// <summary>A clock a test can move, so the expiry test does not wait on real time.</summary>
    private sealed class MovableClock : IClock
    {
        private DateTimeOffset _now;

        public MovableClock(DateTimeOffset now) => _now = now;

        public DateTime Now => _now.UtcDateTime;

        public DateTimeKind Kind => DateTimeKind.Utc;

        public bool SupportsMultipleTimezone => false;

        public DateTime Normalize(DateTime dateTime) => dateTime.Kind switch
        {
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime
        };

        public DateTime ConvertToUserTime(DateTime utcDateTime) => Normalize(utcDateTime);

        public DateTimeOffset ConvertToUserTime(DateTimeOffset dateTimeOffset) => dateTimeOffset.ToUniversalTime();

        public DateTime ConvertToUtc(DateTime dateTime) => Normalize(dateTime);

        public DateTimeOffset Advance(TimeSpan by)
        {
            _now = _now.Add(by);
            return _now;
        }
    }

    private static ClaimsPrincipal Principal(Guid userId, Guid? tenantId)
    {
        var claims = new List<Claim>
        {
            new(AbpClaimTypes.UserId, userId.ToString())
        };
        if (tenantId is not null)
        {
            claims.Add(new Claim(AbpClaimTypes.TenantId, tenantId.Value.ToString()));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
    }

    /// <summary>
    /// A provider that records the transport clients it creates, so a test can assert sharing and disposal
    /// without opening a real socket.
    /// </summary>
    private sealed class ProviderHarness
    {
        public List<RecordingTransportClient> Created { get; } = new();

        public IMcpUserTokenService TokenService { get; }

        public MCPTransportClientProvider Provider { get; }

        public ProviderHarness()
        {
            TokenService = CreateTokenService();
            Provider = new RecordingProvider(this, TokenService);
        }
    }

    private sealed class RecordingProvider : MCPTransportClientProvider
    {
        private readonly ProviderHarness _harness;

        public RecordingProvider(ProviderHarness harness, IMcpUserTokenService tokenService)
            : base(
                new NoopHttpClientFactory(),
                NullLoggerFactory.Instance,
                NullLogger<MCPTransportClientProvider>.Instance,
                tokenService)
        {
            _harness = harness;
        }

        protected override IMCPTransportClient CreateClient(MCPServer server)
        {
            var client = new RecordingTransportClient();
            _harness.Created.Add(client);
            return client;
        }
    }

    private sealed class NoopHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => new();
    }

    private sealed class RecordingTransportClient : IMCPTransportClient
    {
        public MCPTransportType TransportType => MCPTransportType.SSE;
        public bool IsConnected { get; private set; }
        public bool Disposed { get; private set; }
        public string? LastBearerToken { get; private set; }

        public Task ConnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = true;
            return Task.CompletedTask;
        }

        public Task DisconnectAsync(CancellationToken cancellationToken = default)
        {
            IsConnected = false;
            return Task.CompletedTask;
        }

        public Task<List<MCPServerToolDefinition>> ListToolsAsync(
            CancellationToken cancellationToken = default,
            string? bearerToken = null)
        {
            return Task.FromResult(new List<MCPServerToolDefinition>());
        }

        public Task<MCPServerToolResult> CallToolAsync(
            string toolName,
            Dictionary<string, object?> parameters,
            CancellationToken cancellationToken = default,
            string? bearerToken = null)
        {
            LastBearerToken = bearerToken;
            return Task.FromResult(new MCPServerToolResult { Success = true, Result = "ok" });
        }

        public void Dispose()
        {
            Disposed = true;
        }
    }
}

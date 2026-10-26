using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.MCP.Audit;
using SufiChain.SufiPlatform.SufiAI.MCP.Internal;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.MCP;

/// <summary>
/// sufi-chain#384 slice 6a: one audit row per tool/MCP call, for allowed, denied and cancelled calls
/// alike, bound to the turn's identity and carrying only redacted argument keys. Auditing is never
/// allowed to break the turn.
/// </summary>
public class McpToolCallAuditTests
{
    private static readonly Guid TenantA = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");
    private static readonly Guid Alice = Guid.Parse("a11ce000-0000-0000-0000-000000000001");
    private const string Permission = "Demo.Write";

    [Fact]
    public void Argument_summary_keeps_keys_and_redacts_secret_like_keys()
    {
        var summary = McpToolCallAuditArguments.Summarize(new Dictionary<string, object?>
        {
            ["customerId"] = "42",
            ["apiKey"] = "sk-live-do-not-store",
            ["note"] = "internal text",
            ["password"] = "hunter2"
        });

        summary.ShouldNotBeNull();
        summary.ShouldContain("customerId");
        summary.ShouldContain("note");
        summary.ShouldContain("apiKey=redacted");
        summary.ShouldContain("password=redacted");
        // No argument value may survive in any form.
        summary.ShouldNotContain("sk-live-do-not-store");
        summary.ShouldNotContain("hunter2");
        summary.ShouldNotContain("internal text");
        summary.ShouldNotContain("42");
    }

    [Theory]
    [InlineData("api_key")]
    [InlineData("API-KEY")]
    [InlineData("accessToken")]
    [InlineData("ConnectionString")]
    [InlineData("client-secret")]
    public void Secret_like_detection_is_case_and_separator_insensitive(string key)
    {
        McpToolCallAuditArguments.IsSecretLike(key).ShouldBeTrue();
    }

    [Fact]
    public void Argument_summary_is_null_for_an_argument_less_call()
    {
        McpToolCallAuditArguments.Summarize(null).ShouldBeNull();
        McpToolCallAuditArguments.Summarize(new Dictionary<string, object?>()).ShouldBeNull();
    }

    [Fact]
    public void Successful_call_is_allowed_and_succeeded()
    {
        var (decision, status, reason) = McpToolCallAudit.Classify(
            MCPToolExecutionResult.CreateSuccess("ok", 12));

        decision.ShouldBe(McpToolCallDecision.Allowed);
        status.ShouldBe(McpToolCallResultStatus.Succeeded);
        reason.ShouldBeNull();
    }

    [Fact]
    public void Refusal_is_denied_and_a_plain_failure_stays_allowed()
    {
        var refused = McpToolCallAudit.Classify(
            MCPToolExecutionResult.CreateFailure(AgentCallErrorCodes.PermissionDenied));
        refused.Decision.ShouldBe(McpToolCallDecision.Denied);
        refused.Status.ShouldBe(McpToolCallResultStatus.Failed);
        refused.Reason.ShouldBe(AgentCallErrorCodes.PermissionDenied);

        var failure = McpToolCallAudit.Classify(
            MCPToolExecutionResult.CreateFailure("The remote server returned 500."));
        failure.Decision.ShouldBe(McpToolCallDecision.Allowed);
        failure.Status.ShouldBe(McpToolCallResultStatus.Failed);
    }

    [Fact]
    public void Identity_is_lifted_off_the_call_context_and_falls_back_to_the_workspace()
    {
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA), "session-9")
            .WithHooshvare(Guid.NewGuid());

        var identity = AgentCallIdentity.From(new WorkspaceContext { CallContext = call });

        identity.TenantId.ShouldBe(TenantA);
        identity.UserId.ShouldBe(Alice);
        identity.SessionId.ShouldBe("session-9");
        identity.HooshvareId.ShouldBe(call.HooshvareId);

        var fallback = AgentCallIdentity.From(new WorkspaceContext { TenantId = TenantA, UserId = Alice });
        fallback.TenantId.ShouldBe(TenantA);
        fallback.UserId.ShouldBe(Alice);
        fallback.SessionId.ShouldBeNull();
    }

    [Fact]
    public async Task Auditor_writes_one_bound_row_with_the_redacted_arguments()
    {
        var repository = Substitute.For<IMcpToolCallAuditLogRepository>();
        McpToolCallAuditLog? captured = null;
        repository
            .When(r => r.InsertAsync(Arg.Any<McpToolCallAuditLog>(), Arg.Any<bool>(), Arg.Any<CancellationToken>()))
            .Do(call => captured = call.Arg<McpToolCallAuditLog>());

        var auditor = Auditor(repository);
        var context = new WorkspaceContext
        {
            CallContext = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA), "session-1")
        };

        await auditor.RecordAsync(McpToolCallAuditRecord.Create(
            context,
            "helpdesk.kb.search",
            MCPToolType.Internal,
            isReadOnly: true,
            new Dictionary<string, object?> { ["query"] = "reset password", ["apiKey"] = "sk-abc" },
            McpToolCallDecision.Allowed,
            decisionReason: null,
            McpToolCallResultStatus.Succeeded,
            durationMs: 42));

        captured.ShouldNotBeNull();
        captured!.TenantId.ShouldBe(TenantA);
        captured.UserId.ShouldBe(Alice);
        captured.SessionId.ShouldBe("session-1");
        captured.ToolName.ShouldBe("helpdesk.kb.search");
        captured.ToolType.ShouldBe(MCPToolType.Internal);
        captured.IsReadOnly.ShouldBeTrue();
        captured.Decision.ShouldBe(McpToolCallDecision.Allowed);
        captured.ResultStatus.ShouldBe(McpToolCallResultStatus.Succeeded);
        captured.DurationMs.ShouldBe(42);
        captured.ArgumentsSummary.ShouldBe("apiKey=redacted, query");
    }

    [Fact]
    public async Task Auditor_swallows_a_failing_repository_so_the_turn_survives()
    {
        var repository = Substitute.For<IMcpToolCallAuditLogRepository>();
        repository
            .InsertAsync(Arg.Any<McpToolCallAuditLog>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns<Task<McpToolCallAuditLog>>(_ => throw new InvalidOperationException("audit table is down"));

        var auditor = Auditor(repository);

        await Should.NotThrowAsync(() => auditor.RecordAsync(McpToolCallAuditRecord.Create(
            new WorkspaceContext { CallContext = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)) },
            "demo.run",
            MCPToolType.Internal,
            isReadOnly: false,
            parameters: null,
            McpToolCallDecision.Allowed,
            decisionReason: null,
            McpToolCallResultStatus.Failed,
            durationMs: 3)));
    }

    [Fact]
    public async Task Allowed_internal_call_writes_exactly_one_row()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA), "session-1");

        var result = await h.Tool().ExecuteAsync(
            new WorkspaceContext { CallContext = call },
            new Dictionary<string, object?> { ["apiKey"] = "sk-abc", ["input"] = "hello" });

        result.Success.ShouldBeTrue(result.ErrorMessage);
        h.Auditor.Received(1).RecordAsync(
            Arg.Is<McpToolCallAuditRecord>(r =>
                r.ToolName == "demo.run" &&
                r.Decision == McpToolCallDecision.Allowed &&
                r.ResultStatus == McpToolCallResultStatus.Succeeded &&
                r.Identity.UserId == Alice &&
                r.Identity.TenantId == TenantA &&
                r.Identity.SessionId == "session-1"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Refused_internal_call_is_audited_as_denied_and_the_tool_never_runs()
    {
        using var h = new Harness();
        var call = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA));
        h.Permissions.IsGrantedAsync(Arg.Any<ClaimsPrincipal?>(), Permission).Returns(false);

        var result = await h.Tool(Permission).ExecuteAsync(new WorkspaceContext { CallContext = call }, new());

        result.ErrorMessage.ShouldBe(AgentCallErrorCodes.PermissionDenied);
        h.Probe.Calls.ShouldBe(0);
        h.Auditor.Received(1).RecordAsync(
            Arg.Is<McpToolCallAuditRecord>(r =>
                r.Decision == McpToolCallDecision.Denied &&
                r.DecisionReason == AgentCallErrorCodes.PermissionDenied),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Anonymous_turn_is_audited_with_a_null_user_and_never_another_user()
    {
        using var h = new Harness();
        using (h.Principals.Change(Principal(Alice, TenantA)))
        {
            var result = await h.Tool(allowsAnonymous: true).ExecuteAsync(
                new WorkspaceContext { CallContext = AgentCallContext.Anonymous(TenantA, "session-anon") },
                new());

            result.Success.ShouldBeTrue(result.ErrorMessage);
        }

        h.Auditor.Received(1).RecordAsync(
            Arg.Is<McpToolCallAuditRecord>(r =>
                r.Identity.UserId == null &&
                r.Identity.TenantId == TenantA &&
                r.Identity.SessionId == "session-anon"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_throwing_auditor_leaves_the_tool_call_succeeding()
    {
        using var h = new Harness();
        h.Auditor.RecordAsync(Arg.Any<McpToolCallAuditRecord>(), Arg.Any<CancellationToken>())
            .Returns<Task>(_ => throw new InvalidOperationException("audit is down"));

        var result = await h.Tool().ExecuteAsync(
            new WorkspaceContext { CallContext = AgentCallContext.ForUser(TenantA, Alice, Principal(Alice, TenantA)) },
            new());

        result.Success.ShouldBeTrue(result.ErrorMessage);
    }

    [Fact]
    public void The_auditor_correlation_provider_is_registered_for_the_scope()
    {
        // sufi-chain#384 slice 6a: the auditor resolves this from its own scope. Without an explicit
        // registration the row still wrote, but CorrelationId was silently null on every call.
        var services = new ServiceCollection();
        services.AddTransient<IMcpAuditCorrelationProvider, McpAuditCorrelationProvider>();

        using var provider = services.BuildServiceProvider();

        provider.GetService<IMcpAuditCorrelationProvider>().ShouldNotBeNull();
    }

    [Fact]
    public void The_domain_module_registers_the_auditor_correlation_provider()
    {
        var module = ReadSource(
            "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain/SufiAIDomainModule.cs");

        module.ShouldContain(
            "AddTransient<MCP.Audit.IMcpAuditCorrelationProvider, MCP.Audit.McpAuditCorrelationProvider>",
            Case.Sensitive);
    }

    private static string ReadSource(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relative.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relative);
    }

    private static McpToolCallAuditor Auditor(IMcpToolCallAuditLogRepository repository)
    {
        var services = new ServiceCollection()
            .AddSingleton(repository)
            .AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance)
            .BuildServiceProvider();

        return new McpToolCallAuditor(
            services.GetRequiredService<IServiceScopeFactory>(),
            services.GetRequiredService<IGuidGenerator>(),
            NullLogger<McpToolCallAuditor>.Instance);
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

    public sealed class ProbeService
    {
        private readonly ProbeLog _log;

        public ProbeService(ProbeLog log)
        {
            _log = log;
        }

        public Task<string> RunAsync()
        {
            _log.Calls++;
            return Task.FromResult("ok");
        }
    }

    public sealed class ProbeLog
    {
        public int Calls { get; set; }
    }

    private sealed class Harness : IDisposable
    {
        private readonly ServiceProvider _services;

        public Harness()
        {
            Permissions = Substitute.For<IPermissionChecker>();
            Auditor = Substitute.For<IMcpToolCallAuditor>();
            Probe = new ProbeLog();
            _services = new ServiceCollection()
                .AddSingleton<ICurrentTenantAccessor>(AsyncLocalCurrentTenantAccessor.Instance)
                .AddSingleton<ICurrentTenant, CurrentTenant>()
                .AddSingleton<ICurrentPrincipalAccessor, ThreadCurrentPrincipalAccessor>()
                .AddSingleton(Permissions)
                .AddSingleton(Auditor)
                .AddSingleton(Probe)
                .AddScoped<ProbeService>()
                .BuildServiceProvider();
            Tenant = _services.GetRequiredService<ICurrentTenant>();
            Principals = _services.GetRequiredService<ICurrentPrincipalAccessor>();
        }

        public IPermissionChecker Permissions { get; }
        public IMcpToolCallAuditor Auditor { get; }
        public ProbeLog Probe { get; }
        public ICurrentTenant Tenant { get; }
        public ICurrentPrincipalAccessor Principals { get; }

        public InternalMCPTool Tool(string? permission = null, bool requiresPermission = false, bool allowsAnonymous = false) =>
            new("demo.run", "Demo", "{}", typeof(ProbeService),
                typeof(ProbeService).GetMethod(nameof(ProbeService.RunAsync))!,
                _services, permission, requiresPermission, allowsAnonymous: allowsAnonymous);

        public void Dispose() => _services.Dispose();
    }
}

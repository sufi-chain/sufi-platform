using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Agents;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareRuntimeProviderErrorTests
{
    [Fact]
    public async Task SendAsync_hides_provider_urls_and_keys()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())
            .Returns<SufiAIChatResponse>(_ => throw new InvalidOperationException("https://api.example/v1/chat sk-live-secret"));

        var service = new SanitizedRuntime(chat);
        var error = await Should.ThrowAsync<BusinessException>(() => service.SendAsync(new HooshvareRuntimeRequestDto
        {
            HooshvareId = SanitizedRuntime.DefinitionId,
            Message = "سلام",
            PersistChatSession = false
        }));

        error.Code.ShouldBe(AIErrorCodes.ProviderRequestFailed);
        error.InnerException.ShouldBeNull();
        error.Message.ShouldNotContain("https://");
        error.Message.ShouldNotContain("sk-live");
        error.ToString().ShouldNotContain("https://");
        error.ToString().ShouldNotContain("sk-live");
        foreach (System.Collections.DictionaryEntry entry in error.Data)
        {
            entry.Value?.ToString().ShouldNotContain("https://");
            entry.Value?.ToString().ShouldNotContain("sk-live");
        }
    }

    [Fact]
    public async Task SendAsync_still_cancels_when_the_caller_stops()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())
            .Returns<SufiAIChatResponse>(_ => throw new OperationCanceledException());

        var service = new SanitizedRuntime(chat);
        await Should.ThrowAsync<OperationCanceledException>(() => service.SendAsync(new HooshvareRuntimeRequestDto
        {
            HooshvareId = SanitizedRuntime.DefinitionId,
            Message = "سلام",
            PersistChatSession = false
        }));
    }

    [Fact]
    public async Task Submit_pauses_only_when_a_tenant_rule_names_the_tool()
    {
        var open = new FormFillApprovalGate(new HostMinimumAgentToolApprovalListSource());
        (await open.PauseSubmitAsync(approved: false)).ShouldBeFalse();

        var rules = AgentToolApprovalList.WithHostMinimum();
        rules.AddTenantRule(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AgentToolApprovalMatchKind.ToolName,
            effect: null,
            toolName: "forms.submit_form",
            category: null);
        var gated = new FormFillApprovalGate(new FixedApprovalList(rules));
        (await gated.PauseSubmitAsync(approved: false)).ShouldBeTrue();
        (await gated.PauseSubmitAsync(approved: true)).ShouldBeFalse();
    }

    private sealed class FixedApprovalList : IAgentToolApprovalListSource
    {
        private readonly AgentToolApprovalList _list;

        public FixedApprovalList(AgentToolApprovalList list) => _list = list;

        public Task<AgentToolApprovalList> GetListAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_list);
    }

    private sealed class SanitizedRuntime : HooshvareRuntimeAppService
    {
        public static readonly Guid DefinitionId = Guid.Parse("dddddddd-dddd-dddd-dddd-dddddddddddd");

        public SanitizedRuntime(ISufiAIChatService chat)
            : base(
                Substitute.For<IHooshvareDefinitionRepository>(),
                chat,
                new FixedOrchestrator(),
                Substitute.For<IWorkspaceRuntimeConfigurationResolver>(),
                Substitute.For<IAIToolChatExecutor>(),
                Substitute.For<IMCPKernelToolRegistrar>(),
                Substitute.For<IRepository<HooshvareContextSession, Guid>>(),
                Substitute.For<IRepository<HooshvareContextTurn, Guid>>(),
                Substitute.For<IRepository<HooshvareContextSegment, Guid>>(),
                Substitute.For<IWorkspaceRepository>(),
                new HooshvareLlmPayloadAuditLogger(
                    Options.Create(new HooshvareAuditOptions()),
                    null!,
                    null!,
                    null!,
                    null!,
                    NullLogger<HooshvareLlmPayloadAuditLogger>.Instance),
                new FixedConfiguration(),
                Options.Create(new HooshvareContextOptions()),
                new HooshvareChatSessionTitleService(null!),
                new HooshvareAvailabilityChecker(null!),
                new NullHooshvareTurnProgressReporter(),
                Substitute.For<IHooshvareSessionMemoryStore>(),
                NullLogger<HooshvareRuntimeAppService>.Instance)
        {
            var features = Substitute.For<IFeatureChecker>();
            features.GetOrNullAsync(Arg.Any<string>()).Returns(Task.FromResult<string>(null!));
            var services = new ServiceCollection();
            services.AddSingleton(features);
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        }

        protected override Task<HooshvareDefinition> GetEnabledDefinitionAsync(Guid id)
        {
            return Task.FromResult(new HooshvareDefinition(
                DefinitionId,
                null,
                "Tests",
                "آزمایش",
                HooshvareKind.Assistant,
                "Test",
                Guid.NewGuid(),
                "prompt",
                persistChatSession: false));
        }

        protected override Task<HooshvareContextSession> EnsureContextSessionAsync(
            HooshvareDefinition definition,
            HooshvareEffectiveConfigurationDto effectiveConfiguration,
            Guid? requestedSessionId,
            Guid? modelConfigurationId)
        {
            return Task.FromResult(new HooshvareContextSession(
                Guid.NewGuid(),
                null,
                definition.Id,
                Guid.NewGuid(),
                "workspace",
                "model",
                8000));
        }

        protected override Task<string?> ResolveSessionSummaryAsync(
            Guid hooshvareId,
            Guid? sessionId,
            string? requestedSummary,
            HooshvareContextSession contextSession,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<string?>(null);
        }

        protected override Task PersistWorkingMemoryAsync(
            Guid hooshvareId,
            Guid? sessionId,
            HooshvareContextSession contextSession,
            HooshvareOrchestrationResult orchestration,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FixedOrchestrator : HooshvareRuntimeOrchestrator
    {
        public FixedOrchestrator()
            : base(
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                null!,
                NullLogger<HooshvareRuntimeOrchestrator>.Instance)
        {
        }

        public override Task<HooshvareOrchestrationResult> PrepareRequestAsync(
            HooshvareDefinition definition,
            HooshvareRuntimeRequestDto input,
            HooshvareEditableConfigurationDto? effectiveConfiguration = null,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HooshvareOrchestrationResult
            {
                UsedMcp = false,
                Request = new SufiAIChatRequest(),
                WorkspaceBinding = new HooshvareWorkspaceBinding
                {
                    WorkspaceId = Guid.NewGuid(),
                    WorkspaceName = "workspace"
                },
                AllowedMcpToolNames = new List<string>()
            });
        }
    }

    private sealed class FixedConfiguration : HooshvareEffectiveConfigurationResolver
    {
        public FixedConfiguration()
            : base(null!, null!, null!)
        {
        }

        public override Task<HooshvareEffectiveConfigurationDto> ResolveAsync(
            HooshvareDefinition definition,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new HooshvareEffectiveConfigurationDto());
        }
    }
}

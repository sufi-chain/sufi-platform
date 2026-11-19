using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Features;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using SufiChain.SufiPlatform.SufiCom.Chat.Integration;
using SufiChain.SufiPlatform.SufiCom.Chat.Usage;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Xunit;
using FeatureChecker = SufiChain.SufiPlatform.Features.IFeatureChecker;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Agents;

public class AgentPlanEntryTests
{
    [Fact]
    public async Task Playground_preview_rejects_an_agent_when_the_feature_is_off()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        var service = new GatedPlayground(chat);
        service.Use(Entitlement(enabled: false));

        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.SendPreviewAsync(Guid.NewGuid(), new HooshvarePlaygroundPreviewInput { Message = "hello" }));

        error.Code.ShouldBe(AIHooshvareErrorCodes.AgentNotInPlan);
        await chat.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    [Fact]
    public async Task Runtime_send_rejects_an_agent_when_the_feature_is_off()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        var service = new GatedRuntime(chat);
        service.Use(Entitlement(enabled: false));

        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.SendAsync(new HooshvareRuntimeRequestDto
            {
                HooshvareId = GatedRuntime.DefinitionId,
                Message = "hello",
                PersistChatSession = false
            }));

        error.Code.ShouldBe(AIHooshvareErrorCodes.AgentNotInPlan);
        await chat.DidNotReceiveWithAnyArgs().CompleteAsync(default!, default);
    }

    [Fact]
    public async Task Live_agent_start_rejects_when_the_feature_is_off()
    {
        var coordinator = new LiveAgentRunCoordinator(
            Substitute.For<IRepository<AgentRun, Guid>>(),
            Substitute.For<Volo.Abp.Features.IFeatureChecker>(),
            null!,
            null!,
            Substitute.For<Volo.Abp.BackgroundJobs.IBackgroundJobManager>(),
            Substitute.For<Volo.Abp.MultiTenancy.ICurrentTenant>(),
            Substitute.For<Volo.Abp.Users.ICurrentUser>(),
            SimpleGuidGenerator.Instance,
            Substitute.For<Volo.Abp.Timing.IClock>(),
            Entitlement(enabled: false));

        var error = await Should.ThrowAsync<BusinessException>(() =>
            coordinator.BeginAsync("SufiComChat:HomeAgent", Guid.NewGuid(), "hello"));

        error.Code.ShouldBe(AIHooshvareErrorCodes.AgentNotInPlan);
    }

    private static ChatAgentTurnEntitlement Entitlement(bool enabled)
    {
        var features = Substitute.For<FeatureChecker>();
        features.IsEnabledAsync(Arg.Any<string>()).Returns(enabled);
        return new ChatAgentTurnEntitlement(features, Substitute.For<IChatUsageGuard>());
    }

    private sealed class GatedPlayground : HooshvarePlaygroundAppService
    {
        public GatedPlayground(ISufiAIChatService chat)
            : base(
                Substitute.For<IHooshvareDefinitionRepository>(),
                Substitute.For<IHooshvarePlaygroundSessionStateRepository>(),
                Substitute.For<IHooshvareImprovementProposalRepository>(),
                Substitute.For<IHooshvareDefinitionOverrideRepository>(),
                Substitute.For<IChatIntegrationService>(),
                Substitute.For<IWorkspaceRepository>(),
                Substitute.For<IMCPToolRegistry>(),
                Substitute.For<ISufiAIRagService>(),
                chat,
                null!,
                null!,
                Substitute.For<IWorkspaceRuntimeConfigurationResolver>(),
                Substitute.For<IAIToolChatExecutor>(),
                Substitute.For<IMCPKernelToolRegistrar>(),
                null!,
                null!,
                Substitute.For<IHooshvareRagProjectBindingRepository>(),
                NullLogger<HooshvarePlaygroundAppService>.Instance)
        {
        }

        public void Use(IChatAgentTurnEntitlement entitlement)
        {
            var services = new ServiceCollection();
            services.AddSingleton(entitlement);
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        }

        protected override Task CheckBaseAccessAsync() => Task.CompletedTask;

        protected override Task<HooshvarePlaygroundSessionState> GetOwnedStateAsync(Guid sessionId)
        {
            return Task.FromResult(new HooshvarePlaygroundSessionState(
                Guid.NewGuid(),
                null,
                sessionId,
                GatedRuntime.DefinitionId,
                Guid.NewGuid(),
                "{}",
                "fingerprint"));
        }

        protected override Task<HooshvareDefinition> LoadDefinitionAsync(Guid hooshvareId)
        {
            return Task.FromResult(new HooshvareDefinition(
                hooshvareId,
                null,
                "SufiCom.Chat",
                "Home Agent",
                HooshvareKind.Agent,
                "HomeAgent",
                Guid.NewGuid(),
                "prompt",
                persistChatSession: false,
                key: "SufiComChat:HomeAgent"));
        }
    }

    private sealed class GatedRuntime : HooshvareRuntimeAppService
    {
        public static readonly Guid DefinitionId = Guid.Parse("dddddddd-dddd-dddd-dddd-ddddddddddde");

        public GatedRuntime(ISufiAIChatService chat)
            : base(
                Substitute.For<IHooshvareDefinitionRepository>(),
                chat,
                null!,
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
                null!,
                Options.Create(new HooshvareContextOptions()),
                null!,
                null!,
                new NullHooshvareTurnProgressReporter(),
                Substitute.For<IHooshvareSessionMemoryStore>(),
                NullLogger<HooshvareRuntimeAppService>.Instance)
        {
        }

        public void Use(IChatAgentTurnEntitlement entitlement)
        {
            var services = new ServiceCollection();
            services.AddSingleton(entitlement);
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider());
        }

        protected override Task<HooshvareDefinition> GetEnabledDefinitionAsync(Guid id)
        {
            return Task.FromResult(new HooshvareDefinition(
                DefinitionId,
                null,
                "SufiCom.Chat",
                "Home Agent",
                HooshvareKind.Agent,
                "HomeAgent",
                Guid.NewGuid(),
                "prompt",
                persistChatSession: false,
                key: "SufiComChat:HomeAgent"));
        }
    }
}

using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRuntimeModelSelectionTests
{
    [Fact]
    public async Task Should_Copy_ModelConfigurationId_Onto_Chat_Request_And_Resolver_Selection()
    {
        var routeId = Guid.NewGuid();
        var (definition, runtimeResolver, orchestrator) = CreateOrchestrator(
            useMcpTools: false,
            allowUserModelSelection: true);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = definition.Id,
            Message = "Hello",
            ModelConfigurationId = routeId
        };

        var result = await orchestrator.PrepareRequestAsync(definition, input);

        result.Request.ModelConfigurationId.ShouldBe(routeId);
        result.RuntimeConfiguration.ModelConfigurationId.ShouldBe(routeId);
        await runtimeResolver.Received(1).ResolveAsync(
            definition.WorkspaceId,
            AICapabilityType.ChatCompletion,
            Arg.Is<AIModelRouteSelection>(selection =>
                selection.ModelConfigurationId == routeId && !selection.RequiresToolCalling),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Require_Tool_Calling_When_Mcp_Tools_Are_Enabled()
    {
        var routeId = Guid.NewGuid();
        var (definition, runtimeResolver, orchestrator) = CreateOrchestrator(
            useMcpTools: true,
            allowUserModelSelection: true);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = definition.Id,
            Message = "Use a tool",
            ModelConfigurationId = routeId
        };

        var result = await orchestrator.PrepareRequestAsync(definition, input);

        result.Request.ModelConfigurationId.ShouldBe(routeId);
        result.UsedMcp.ShouldBeTrue();
        await runtimeResolver.Received(1).ResolveAsync(
            definition.WorkspaceId,
            AICapabilityType.ChatCompletion,
            Arg.Is<AIModelRouteSelection>(selection =>
                selection.ModelConfigurationId == routeId && selection.RequiresToolCalling),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Reject_Explicit_Route_When_Copilot_Disallows_User_Selection()
    {
        var routeId = Guid.NewGuid();
        var (definition, _, orchestrator) = CreateOrchestrator(useMcpTools: false);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = definition.Id,
            Message = "Hello",
            ModelConfigurationId = routeId
        };

        var exception = await Should.ThrowAsync<Volo.Abp.BusinessException>(
            () => orchestrator.PrepareRequestAsync(definition, input));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteNotAllowedForCopilot);
    }

    [Fact]
    public async Task Should_Pass_Definition_Allowlist_To_Resolver()
    {
        var routeId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var (definition, runtimeResolver, orchestrator) = CreateOrchestrator(
            useMcpTools: false,
            allowUserModelSelection: true,
            allowedModelConfigurationIds: [routeId, otherId]);
        var input = new CopilotRuntimeRequestDto
        {
            CopilotId = definition.Id,
            Message = "Hello",
            ModelConfigurationId = routeId
        };

        await orchestrator.PrepareRequestAsync(definition, input);

        await runtimeResolver.Received(1).ResolveAsync(
            definition.WorkspaceId,
            AICapabilityType.ChatCompletion,
            Arg.Is<AIModelRouteSelection>(selection =>
                selection.ModelConfigurationId == routeId &&
                selection.AllowedModelConfigurationIds != null &&
                selection.AllowedModelConfigurationIds.Count == 2 &&
                selection.AllowedModelConfigurationIds.Contains(routeId) &&
                selection.AllowedModelConfigurationIds.Contains(otherId)),
            Arg.Any<CancellationToken>());
    }

    private static (CopilotDefinition Definition, IWorkspaceRuntimeConfigurationResolver RuntimeResolver, CopilotRuntimeOrchestrator Orchestrator) CreateOrchestrator(
        bool useMcpTools,
        bool allowUserModelSelection = false,
        IReadOnlyCollection<Guid>? allowedModelConfigurationIds = null)
    {
        var definition = new CopilotDefinition(
            Guid.NewGuid(),
            null,
            "Tests",
            "Test assistant",
            CopilotKind.Assistant,
            "Assistant",
            Guid.NewGuid(),
            "System instructions",
            false);
        definition.SetModelSelectionPolicy(allowUserModelSelection, allowedModelConfigurationIds);
        if (useMcpTools)
        {
            definition.SetRuntimeOptions(new CopilotRuntimeOptions
            {
                UseMcpTools = true,
                AllowedMcpToolNames = ["test_tool"]
            });
        }

        var tool = Substitute.For<IMCPTool>();
        tool.Name.Returns("test_tool");
        tool.Description.Returns("Test tool description");
        var tools = Substitute.For<IMCPToolRegistry>();
        tools.ResolveAsync(Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
            .Returns(new MCPToolResolutionResult { Tools = [tool] });
        var resolver = Substitute.For<ICopilotWorkspaceResolver>();
        resolver.ResolveAsync(definition.WorkspaceId, Arg.Any<CancellationToken>())
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = definition.WorkspaceId,
                WorkspaceName = "workspace",
                IsActive = true,
                IsReady = true
            });
        var runtimeResolver = Substitute.For<IWorkspaceRuntimeConfigurationResolver>();
        runtimeResolver
            .ResolveAsync(
                Arg.Any<Guid>(),
                Arg.Any<AICapabilityType>(),
                Arg.Any<AIModelRouteSelection>(),
                Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var selection = call.Arg<AIModelRouteSelection>();
                return new WorkspaceRuntimeConfiguration
                {
                    Workspace = new Workspace(definition.WorkspaceId, "workspace", AIProviderType.OpenAI, "gpt-4"),
                    CapabilityType = AICapabilityType.ChatCompletion,
                    Provider = AIProviderType.OpenAI,
                    ModelId = "gpt-4",
                    OpenAIApiMode = OpenAIApiMode.ChatCompletions,
                    ModelConfigurationId = selection.ModelConfigurationId,
                    IsExplicitSelection = selection.ModelConfigurationId.HasValue,
                    IsConfigured = true,
                    IsReady = true
                };
            });

        var orchestrator = new CopilotRuntimeOrchestrator(
            Substitute.For<ISufiAIRagService>(),
            Substitute.For<ICopilotRagProjectBindingRepository>(),
            tools,
            new CopilotBusinessLocalizationService(Substitute.For<IStringLocalizerFactory>()),
            Substitute.For<IStringLocalizerFactory>(),
            resolver,
            runtimeResolver,
            new CopilotContextTokenEstimator(),
            Substitute.For<ICopilotTurnProgressReporter>(),
            Substitute.For<IWorkspaceGuardrailService>(),
            new NullCopilotRagIndexingWorkspaceResolver(),
            Substitute.For<ICopilotContextFieldRegistry>(),
            NullLogger<CopilotRuntimeOrchestrator>.Instance);

        return (definition, runtimeResolver, orchestrator);
    }
}

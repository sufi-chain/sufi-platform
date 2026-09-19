using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRuntimeCancellationTests
{
    [Fact]
    public async Task PrepareRequestAsync_Should_Honor_Cancelled_Token()
    {
        var definition = new CopilotDefinition(
            Guid.NewGuid(),
            null,
            "Tests",
            "Cancelled copilot",
            CopilotKind.Assistant,
            "Test",
            Guid.NewGuid(),
            "prompt",
            persistChatSession: false);

        var workspaceResolver = Substitute.For<ICopilotWorkspaceResolver>();
        workspaceResolver
            .When(x => x.ResolveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()))
            .Do(call => call.Arg<CancellationToken>().ThrowIfCancellationRequested());

        var orchestrator = new CopilotRuntimeOrchestrator(
            CopilotRuntimeTestSupport.CreateRagRetrieval(),
            Substitute.For<ICopilotRagProjectBindingRepository>(),
            Substitute.For<IMCPToolRegistry>(),
            new CopilotBusinessLocalizationService(Substitute.For<IStringLocalizerFactory>()),
            Substitute.For<IStringLocalizerFactory>(),
            workspaceResolver,
            Substitute.For<IWorkspaceRuntimeConfigurationResolver>(),
            new CopilotContextTokenEstimator(),
            Substitute.For<IWorkspaceGuardrailService>(),
            Substitute.For<ICopilotRagIndexingWorkspaceResolver>(),
            Substitute.For<ICopilotContextFieldRegistry>(),
            NullLogger<CopilotRuntimeOrchestrator>.Instance);

        await Should.ThrowAsync<OperationCanceledException>(() =>
            orchestrator.PrepareRequestAsync(
                definition,
                new CopilotRuntimeRequestDto { CopilotId = definition.Id, Message = "hi" },
                cancellationToken: new CancellationToken(canceled: true)));
    }
}

using Microsoft.Extensions.Localization;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using Shouldly;

using SufiChain.SufiPlatform.SufiAI;

using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;

using SufiChain.SufiPlatform.SufiAI.Workspaces;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareRuntimeCancellationTests

{

    [Fact]

    public async Task PrepareRequestAsync_Should_Honor_Cancelled_Token()

    {

        var definition = new HooshvareDefinition(

            Guid.NewGuid(),

            null,

            "Tests",

            "Cancelled hooshvare",

            HooshvareKind.Assistant,

            "Test",

            Guid.NewGuid(),

            "prompt",

            persistChatSession: false);



        var workspaceResolver = Substitute.For<IHooshvareWorkspaceResolver>();

        workspaceResolver

            .When(x => x.ResolveAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()))

            .Do(call => call.Arg<CancellationToken>().ThrowIfCancellationRequested());



        var orchestrator = new HooshvareRuntimeOrchestrator(

            HooshvareRuntimeTestSupport.CreateRagRetrieval(),

            Substitute.For<IHooshvareRagProjectBindingRepository>(),

            Substitute.For<IMCPToolRegistry>(),

            new HooshvareBusinessLocalizationService(Substitute.For<IStringLocalizerFactory>()),

            Substitute.For<IStringLocalizerFactory>(),

            workspaceResolver,

            Substitute.For<IWorkspaceRuntimeConfigurationResolver>(),

            new HooshvareContextTokenEstimator(),

            Substitute.For<IWorkspaceGuardrailService>(),

            Substitute.For<IHooshvareRagIndexingWorkspaceResolver>(),

            Substitute.For<IHooshvareContextFieldRegistry>(),

            NullLogger<HooshvareRuntimeOrchestrator>.Instance);



        await Should.ThrowAsync<OperationCanceledException>(() =>

            orchestrator.PrepareRequestAsync(

                definition,

                new HooshvareRuntimeRequestDto { HooshvareId = definition.Id, Message = "hi" },

                cancellationToken: new CancellationToken(canceled: true)));

    }

}


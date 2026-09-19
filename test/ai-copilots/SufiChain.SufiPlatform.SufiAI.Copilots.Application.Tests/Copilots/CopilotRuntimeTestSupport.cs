using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

internal static class CopilotRuntimeTestSupport
{
    public static CopilotRagRetrievalService CreateRagRetrieval(
        ISufiAIRagService? rag = null,
        bool searchKb = false)
    {
        var planner = Substitute.For<ICopilotRagSearchPlanner>();
        planner.DecideAsync(Arg.Any<CopilotRagPlannerRequest>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var request = call.Arg<CopilotRagPlannerRequest>();
                return new CopilotRagPlannerDecision
                {
                    SearchKb = searchKb,
                    Query = request.Input.Message,
                    Memory = request.PriorMemory,
                    Succeeded = true
                };
            });

        var circuit = Substitute.For<ICopilotRagCircuitStore>();
        circuit.IsOpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);

        return new CopilotRagRetrievalService(
            rag ?? Substitute.For<ISufiAIRagService>(),
            planner,
            circuit,
            Substitute.For<ICopilotTurnProgressReporter>(),
            NullLogger<CopilotRagRetrievalService>.Instance);
    }
}

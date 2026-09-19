using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using SufiChain.SufiPlatform.SufiAI;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



internal static class HooshvareRuntimeTestSupport

{

    public static HooshvareRagRetrievalService CreateRagRetrieval(

        ISufiAIRagService? rag = null,

        bool searchKb = false)

    {

        var planner = Substitute.For<IHooshvareRagSearchPlanner>();

        planner.DecideAsync(Arg.Any<HooshvareRagPlannerRequest>(), Arg.Any<CancellationToken>())

            .Returns(call =>

            {

                var request = call.Arg<HooshvareRagPlannerRequest>();

                return new HooshvareRagPlannerDecision

                {

                    SearchKb = searchKb,

                    Query = request.Input.Message,

                    Memory = request.PriorMemory,

                    Succeeded = true

                };

            });



        var circuit = Substitute.For<IHooshvareRagCircuitStore>();

        circuit.IsOpenAsync(Arg.Any<string>(), Arg.Any<CancellationToken>()).Returns(false);



        return new HooshvareRagRetrievalService(

            rag ?? Substitute.For<ISufiAIRagService>(),

            planner,

            circuit,

            Substitute.For<IHooshvareTurnProgressReporter>(),

            NullLogger<HooshvareRagRetrievalService>.Instance);

    }

}


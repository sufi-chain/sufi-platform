using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class HooshvareDecisionPreflightTests
{
    [Fact]
    public async Task No_route_choice_and_no_rag_or_web_skips_jev()
    {
        var client = Substitute.For<ISystemOneDecisionClient>();
        var preflight = new HooshvareDecisionPreflight([client]);
        var routeA = Guid.NewGuid();

        var result = await preflight.TryAsync(
            Guid.NewGuid(),
            Definition(allowSelection: false),
            new HooshvareRuntimeRequestDto { Message = "hi" },
            new HooshvareRuntimeOptions(),
            [routeA]);

        result.ChatRouteId.ShouldBeNull();
        await client.DidNotReceive().TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Clear_route_choice_selects_that_route()
    {
        var routeA = Guid.NewGuid();
        var routeB = Guid.NewGuid();
        var client = Substitute.For<ISystemOneDecisionClient>();
        client.TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SystemOneDecisionResult
            {
                Answers = new Dictionary<string, SystemOneAnswer>
                {
                    ["route"] = new()
                    {
                        Type = "choice",
                        Choice = routeB.ToString("D"),
                        Confidence = 0.95
                    }
                }
            });
        var preflight = new HooshvareDecisionPreflight([client]);

        var result = await preflight.TryAsync(
            Guid.NewGuid(),
            Definition(allowSelection: true),
            new HooshvareRuntimeRequestDto { Message = "hi" },
            new HooshvareRuntimeOptions(),
            [routeA, routeB]);

        result.ChatRouteId.ShouldBe(routeB);
    }

    [Fact]
    public async Task Unsure_route_choice_leaves_default_route()
    {
        var routeA = Guid.NewGuid();
        var routeB = Guid.NewGuid();
        var client = Substitute.For<ISystemOneDecisionClient>();
        client.TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SystemOneDecisionResult
            {
                Answers = new Dictionary<string, SystemOneAnswer>
                {
                    ["route"] = new()
                    {
                        Type = "choice",
                        Choice = routeB.ToString("D"),
                        Confidence = 0.4
                    }
                }
            });
        var preflight = new HooshvareDecisionPreflight([client]);

        var result = await preflight.TryAsync(
            Guid.NewGuid(),
            Definition(allowSelection: true),
            new HooshvareRuntimeRequestDto { Message = "hi" },
            new HooshvareRuntimeOptions(),
            [routeA, routeB]);

        result.ChatRouteId.ShouldBeNull();
    }

    [Fact]
    public async Task Client_null_leaves_default_route_without_throwing()
    {
        var routeA = Guid.NewGuid();
        var routeB = Guid.NewGuid();
        var client = Substitute.For<ISystemOneDecisionClient>();
        client.TryDecideAsync(Arg.Any<SystemOneDecisionRequest>(), Arg.Any<CancellationToken>())
            .Returns((SystemOneDecisionResult?)null);
        var preflight = new HooshvareDecisionPreflight([client]);

        var result = await preflight.TryAsync(
            Guid.NewGuid(),
            Definition(allowSelection: true),
            new HooshvareRuntimeRequestDto { Message = "hi" },
            new HooshvareRuntimeOptions(),
            [routeA, routeB]);

        result.ChatRouteId.ShouldBeNull();
    }

    private static HooshvareDefinition Definition(bool allowSelection)
    {
        var definition = new HooshvareDefinition(
            Guid.NewGuid(),
            null,
            "tests",
            "Preflight",
            HooshvareKind.Assistant,
            "test",
            Guid.NewGuid(),
            "You help.",
            persistChatSession: false);
        definition.SetModelSelectionPolicy(allowSelection, null);
        return definition;
    }
}

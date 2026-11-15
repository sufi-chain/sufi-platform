using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class NvidiaReasoningEffortTests
{
    [Theory]
    [InlineData("nvidia/nemotron-3-super")]
    [InlineData("openrouter/free")]
    public void High_is_not_sent_when_the_route_rejects_it(string modelId)
    {
        var configuration = Route(modelId, "high,medium,low", "high");

        ChatRouteRequestGuard.ResolveEffort(configuration, "high").ShouldBe("medium");
        ChatRouteRequestGuard.ResolveEffort(configuration, null).ShouldBe("medium");
        ReasoningEffortSelection.ChooseDefault(
            new[] { "high", "medium", "low" },
            "high",
            modelId).ShouldBe("medium");
    }

    [Fact]
    public void An_only_high_list_omits_reasoning_for_nvidia()
    {
        var configuration = Route("nvidia/llama-3.1-nemotron-70b", "high", "high");

        ChatRouteRequestGuard.ResolveEffort(configuration, "high").ShouldBeNull();
        ChatRouteRequestGuard.ResolveEffort(configuration, null).ShouldBeNull();
    }

    [Fact]
    public void A_declared_high_still_stands_for_a_model_that_accepts_it()
    {
        var configuration = Route("openai/gpt-4.1", "high,medium,low", "high");

        ChatRouteRequestGuard.ResolveEffort(configuration, null).ShouldBe("high");
        ReasoningEffortSelection.ChooseDefault(new[] { "high", "medium" }, "high").ShouldBe("high");
    }

    private static AIModelConfiguration Route(string modelId, string efforts, string defaultEffort)
    {
        var configuration = new AIModelConfiguration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.ChatCompletion,
            modelId);
        configuration.SetChatCapabilities(
            acceptsImageInput: null,
            acceptsFileInput: null,
            supportsReasoning: true,
            reasoningEfforts: efforts,
            defaultReasoningEffort: defaultEffort,
            capabilitySource: null);
        return configuration;
    }
}

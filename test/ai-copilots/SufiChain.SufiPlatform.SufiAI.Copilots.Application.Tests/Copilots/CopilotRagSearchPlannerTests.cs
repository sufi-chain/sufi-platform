using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRagSearchPlannerTests
{
    [Fact]
    public async Task Should_Parse_Structured_Planner_Json()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIChatResponse
            {
                Content = """{"searchKb":false,"query":null,"memory":"Greeting; stay on current topic."}"""
            });
        var planner = new CopilotRagSearchPlanner(chat, NullLogger<CopilotRagSearchPlanner>.Instance);

        var decision = await planner.DecideAsync(new CopilotRagPlannerRequest
        {
            Input = new CopilotRuntimeRequestDto { Message = "hello" },
            ChatWorkspaceName = "default",
            PriorMemory = "Earlier topic: billing"
        });

        decision.Succeeded.ShouldBeTrue();
        decision.SearchKb.ShouldBeFalse();
        decision.Memory.ShouldBe("Greeting; stay on current topic.");
    }

    [Fact]
    public async Task Should_Skip_Search_When_Planner_Json_Is_Unparseable()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(new SufiAIChatResponse { Content = "I cannot produce JSON" });
        var planner = new CopilotRagSearchPlanner(chat, NullLogger<CopilotRagSearchPlanner>.Instance);

        var decision = await planner.DecideAsync(new CopilotRagPlannerRequest
        {
            Input = new CopilotRuntimeRequestDto { Message = "hello" },
            ChatWorkspaceName = "default",
            PriorMemory = "Keep this"
        });

        decision.Succeeded.ShouldBeFalse();
        decision.SearchKb.ShouldBeFalse();
        decision.Memory.ShouldBe("Keep this");
    }

    [Fact]
    public async Task Should_Skip_Search_When_Planner_Chat_Fails()
    {
        var chat = Substitute.For<ISufiAIChatService>();
        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);
        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<SufiAIChatResponse>(new InvalidOperationException("route down")));
        var planner = new CopilotRagSearchPlanner(chat, NullLogger<CopilotRagSearchPlanner>.Instance);

        var decision = await planner.DecideAsync(new CopilotRagPlannerRequest
        {
            Input = new CopilotRuntimeRequestDto { Message = "hello" },
            ChatWorkspaceName = "default"
        });

        decision.Succeeded.ShouldBeFalse();
        decision.SearchKb.ShouldBeFalse();
    }
}

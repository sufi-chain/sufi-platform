using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using Shouldly;

using SufiChain.SufiPlatform.SufiAI;

using Xunit;



namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;



public class HooshvareRagSearchPlannerTests

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

        var planner = new HooshvareRagSearchPlanner(chat, NullLogger<HooshvareRagSearchPlanner>.Instance);



        var decision = await planner.DecideAsync(new HooshvareRagPlannerRequest

        {

            Input = new HooshvareRuntimeRequestDto { Message = "hello" },

            ChatWorkspaceName = "default",

            PriorMemory = "Earlier topic: billing"

        });



        decision.Succeeded.ShouldBeTrue();

        decision.SearchKb.ShouldBeFalse();

        decision.Memory.ShouldBe("Greeting; stay on current topic.");

    }



    [Fact]

    public async Task Should_Flag_Invalid_Plan_When_Planner_Json_Is_Unparseable()

    {

        var chat = Substitute.For<ISufiAIChatService>();

        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);

        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())

            .Returns(new SufiAIChatResponse { Content = "I cannot produce JSON" });

        var planner = new HooshvareRagSearchPlanner(chat, NullLogger<HooshvareRagSearchPlanner>.Instance);



        var decision = await planner.DecideAsync(new HooshvareRagPlannerRequest

        {

            Input = new HooshvareRuntimeRequestDto { Message = "hello" },

            ChatWorkspaceName = "default",

            PriorMemory = "Keep this"

        });



        decision.Succeeded.ShouldBeFalse();

        decision.SearchKb.ShouldBeFalse();

        decision.InvalidPlan.ShouldBeTrue();

        decision.Memory.ShouldBe("Keep this");

    }



    [Theory]

    [InlineData("""{"search_kb":"true","search_query":"reset password","memory":"Password reset."}""")]

    [InlineData("""Here is the plan: {"SearchKb": true, "query": "reset password", "memory": "Password reset.",}""")]

    [InlineData("```json\n{\"searchKb\":1,\"query\":\"reset password\",\"memory\":\"Password reset.\"}\n```")]

    public async Task Should_Parse_Tolerant_Planner_Json(string content)

    {

        var chat = Substitute.For<ISufiAIChatService>();

        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);

        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())

            .Returns(new SufiAIChatResponse { Content = content });

        var planner = new HooshvareRagSearchPlanner(chat, NullLogger<HooshvareRagSearchPlanner>.Instance);



        var decision = await planner.DecideAsync(new HooshvareRagPlannerRequest

        {

            Input = new HooshvareRuntimeRequestDto { Message = "how do I reset a password?" },

            ChatWorkspaceName = "default"

        });



        decision.Succeeded.ShouldBeTrue();

        decision.InvalidPlan.ShouldBeFalse();

        decision.SearchKb.ShouldBeTrue();

        decision.Query.ShouldBe("reset password");

        decision.Memory.ShouldBe("Password reset.");

    }



    [Fact]

    public async Task Should_Skip_Search_When_Planner_Chat_Fails()

    {

        var chat = Substitute.For<ISufiAIChatService>();

        chat.IsAvailableAsync(Arg.Any<CancellationToken>()).Returns(true);

        chat.CompleteAsync(Arg.Any<SufiAIChatRequest>(), Arg.Any<CancellationToken>())

            .Returns(Task.FromException<SufiAIChatResponse>(new InvalidOperationException("route down")));

        var planner = new HooshvareRagSearchPlanner(chat, NullLogger<HooshvareRagSearchPlanner>.Instance);



        var decision = await planner.DecideAsync(new HooshvareRagPlannerRequest

        {

            Input = new HooshvareRuntimeRequestDto { Message = "hello" },

            ChatWorkspaceName = "default"

        });



        decision.Succeeded.ShouldBeFalse();

        decision.SearchKb.ShouldBeFalse();

    }

}


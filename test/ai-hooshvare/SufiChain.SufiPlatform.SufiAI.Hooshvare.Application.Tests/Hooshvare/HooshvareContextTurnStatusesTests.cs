using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Application.Tests.Hooshvare;

/// <summary>sufi-chain#340: turns stopped at the time or tool-round limit were logged as Success.</summary>
public class HooshvareContextTurnStatusesTests
{
    [Theory]
    [InlineData("time_limit", "Timeout")]
    [InlineData("tool_round_limit", "ToolLimit")]
    [InlineData("stop", "Success")]
    [InlineData(null, "Success")]
    public void Status_follows_the_finish_reason(string? finishReason, string status)
    {
        HooshvareContextTurnStatuses.FromFinishReason(finishReason).ShouldBe(status);
    }

    [Fact]
    public void Limit_turns_carry_the_tool_loop_error_code()
    {
        HooshvareContextTurnStatuses.ErrorFor(AIToolLoopFinishReasons.TimeLimit).ShouldBe(AIErrorCodes.ToolLoopLimitReached);
        HooshvareContextTurnStatuses.ErrorFor(AIToolLoopFinishReasons.ToolRoundLimit).ShouldBe(AIErrorCodes.ToolLoopLimitReached);
        HooshvareContextTurnStatuses.ErrorFor("stop").ShouldBeNull();
        HooshvareContextTurnStatuses.Timeout.Length.ShouldBeLessThanOrEqualTo(64);
        HooshvareContextTurnStatuses.ToolLimit.Length.ShouldBeLessThanOrEqualTo(64);
    }
}

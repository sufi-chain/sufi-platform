using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotRagFollowUpQueryBuilderTests
{
    [Theory]
    [InlineData("این پلتفرم چطور به شرکت تولید و پخش رادیاتور من کمک میکنه؟")]
    [InlineData("لایسنسش چیه؟")]
    [InlineData("بیشتر توضیح بده")]
    [InlineData("What is its license?")]
    [InlineData("Tell me more")]
    [InlineData("كيف تساعدني هذه المنصة؟")]
    [InlineData("¿Cómo ayuda esta plataforma a mi empresa?")]
    [InlineData("WHAT IS ITS LICENSE?")]
    [InlineData("اين پلتفرم چه كمكي مي كند؟")]
    public void Should_Enrich_Explicit_FollowUp_Using_User_Anchor(string message)
    {
        var input = CreateInput(message);

        var query = CopilotRagFollowUpQueryBuilder.TryBuild(input);

        query.ShouldNotBeNull();
        query.ShouldContain(message);
        query.ShouldContain("سکو صوفی چیه؟");
        query.ShouldNotContain("assistant-only-invention");
        input.Message.ShouldBe(message);
        input.ConversationHistory[0].Content.ShouldBe("سکو صوفی چیه؟");
    }

    [Theory]
    [InlineData("سلام")]
    [InlineData("ممنون")]
    [InlineData("ok")]
    [InlineData("👍")]
    [InlineData("قیمت دلار چنده؟")]
    [InlineData("What is the weather in Tehran?")]
    [InlineData("notthis platform")]
    [InlineData("this platformer game")]
    public void Should_Not_Enrich_Unanchored_Current_Message(string message)
    {
        CopilotRagFollowUpQueryBuilder.TryBuild(CreateInput(message)).ShouldBeNull();
    }

    [Fact]
    public void Should_Not_Use_Assistant_System_Summary_Or_Social_Turns_As_Anchors()
    {
        var input = new CopilotRuntimeRequestDto
        {
            Message = "لایسنسش چیه؟",
            SessionSummary = "سکوی صوفی",
            ConversationHistory =
            [
                new() { Role = "system", Content = "سکوی صوفی" },
                new() { Role = "assistant", Content = "سکوی صوفی" },
                new() { Role = "user", Content = "سلام" },
                new() { Role = "user", Content = "ممنون" },
                new() { Role = "user", Content = "ok" },
                new() { Role = "user", Content = " " }
            ]
        };

        CopilotRagFollowUpQueryBuilder.TryBuild(input).ShouldBeNull();
        input.ConversationHistory.Clear();
        CopilotRagFollowUpQueryBuilder.TryBuild(input).ShouldBeNull();
    }

    [Fact]
    public void Should_Not_Reach_Outside_Recent_User_Window_For_An_Anchor()
    {
        var input = CreateInput("لایسنسش چیه؟");
        for (var turnIndex = 0; turnIndex < CopilotRagFollowUpQueryBuilder.MaxHistoryUserTurns; turnIndex++)
        {
            input.ConversationHistory.Add(new CopilotChatMessageDto { Role = "user", Content = "ممنون" });
        }

        CopilotRagFollowUpQueryBuilder.TryBuild(input).ShouldBeNull();
    }

    [Fact]
    public void Should_Use_At_Most_Two_Recent_Substantive_User_Anchors()
    {
        var input = CreateInput("لایسنسش چیه؟");
        input.ConversationHistory.Add(new CopilotChatMessageDto { Role = "user", Content = "recent-anchor-one" });
        input.ConversationHistory.Add(new CopilotChatMessageDto { Role = "user", Content = "recent-anchor-two" });
        input.ConversationHistory.Add(new CopilotChatMessageDto { Role = "user", Content = "ممنون" });

        var query = CopilotRagFollowUpQueryBuilder.TryBuild(input);

        query.ShouldNotBeNull();
        query.ShouldContain("recent-anchor-one");
        query.ShouldContain("recent-anchor-two");
        query.ShouldNotContain("سکو صوفی چیه؟");
    }

    [Fact]
    public void Should_Bound_Anchors_Without_Truncating_Current_Question()
    {
        var input = CreateInput("لایسنسش چیه؟");
        input.ConversationHistory[0].Content = new string('ع', CopilotRagFollowUpQueryBuilder.MaxAnchorCharacters) + "excluded-tail";

        var query = CopilotRagFollowUpQueryBuilder.TryBuild(input);

        query.ShouldNotBeNull();
        query.ShouldContain(input.Message);
        query.ShouldContain(new string('ع', CopilotRagFollowUpQueryBuilder.MaxAnchorCharacters));
        query.ShouldNotContain("excluded-tail");
    }

    [Fact]
    public void Should_Decline_Retry_For_Oversized_Current_Message()
    {
        var input = CreateInput("لایسنسش " + new string('ع', CopilotRagFollowUpQueryBuilder.MaxCurrentMessageCharacters));

        CopilotRagFollowUpQueryBuilder.TryBuild(input).ShouldBeNull();
    }

    [Fact]
    public void Should_Keep_Maximum_Length_Current_Message_Unchanged()
    {
        const string prefix = "لایسنسش ";
        var input = CreateInput(prefix + new string('ع', CopilotRagFollowUpQueryBuilder.MaxCurrentMessageCharacters - prefix.Length));

        var query = CopilotRagFollowUpQueryBuilder.TryBuild(input);

        query.ShouldNotBeNull();
        query.ShouldStartWith(input.Message);
    }

    [Fact]
    public void Should_Not_Retry_When_Only_Anchor_Duplicates_Current_Query()
    {
        var input = CreateInput("What is its license?");
        input.ConversationHistory[0].Content = "WHAT IS ITS LICENSE!";

        CopilotRagFollowUpQueryBuilder.TryBuild(input).ShouldBeNull();
    }

    [Fact]
    public void Should_Deduplicate_Normalized_Historical_Anchors()
    {
        var input = CreateInput("لایسنسش چیه؟");
        input.ConversationHistory.Add(new CopilotChatMessageDto { Role = "user", Content = "سكو صوفي چيه!" });

        var query = CopilotRagFollowUpQueryBuilder.TryBuild(input);

        query.ShouldNotBeNull();
        query.ShouldContain("سكو صوفي چيه!");
        query.ShouldNotContain("سکو صوفی چیه؟");
    }

    private static CopilotRuntimeRequestDto CreateInput(string message)
    {
        return new CopilotRuntimeRequestDto
        {
            Message = message,
            ConversationHistory =
            [
                new() { Role = "user", Content = "سکو صوفی چیه؟" },
                new() { Role = "assistant", Content = "assistant-only-invention" }
            ]
        };
    }
}

using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotConversationTitleBuilderTests
{
    [Fact]
    public void Should_Build_Title_From_User_And_Assistant()
    {
        var title = CopilotConversationTitleBuilder.Build(
            "How do I edit an article?",
            "Open the article editor and update the body.");

        title.ShouldContain("How do I edit an article?");
        title.ShouldContain("Open the article editor");
        title.ShouldContain("—");
    }

    [Fact]
    public void Should_Respect_Max_Length()
    {
        var user = new string('u', 200);
        var assistant = new string('a', 200);

        var title = CopilotConversationTitleBuilder.Build(user, assistant, maxLength: 80);

        title.Length.ShouldBeLessThanOrEqualTo(80);
        title.ShouldStartWith("uu");
        title.ShouldContain("—");
    }

    [Fact]
    public void Should_Use_First_Line_Only()
    {
        var title = CopilotConversationTitleBuilder.Build(
            "First line\nSecond line",
            "Assistant reply");

        title.ShouldStartWith("First line");
        title.ShouldNotContain("Second line");
    }

    [Fact]
    public void Should_Return_Empty_When_Both_Missing()
    {
        CopilotConversationTitleBuilder.Build("   ", null).ShouldBe(string.Empty);
    }
}

using Shouldly;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Data;
using Xunit;

namespace SufiChain.SufiPlatform.Calendar.Hooshvare;

public class CalendarAssistantLiveToolResultTests
{
    [Fact]
    public void AssistantInstructions_PreferLatestToolResults_DoNotCiteDeletedEvent()
    {
        var prompt = CalendarAssistantHooshvareSeedTexts.Texts.SystemPrompt["en"];

        prompt.ShouldContain("latest search_events result");
        prompt.ShouldContain("deleted event is not busy");
        prompt.ShouldNotContain("Copilot");
    }
}

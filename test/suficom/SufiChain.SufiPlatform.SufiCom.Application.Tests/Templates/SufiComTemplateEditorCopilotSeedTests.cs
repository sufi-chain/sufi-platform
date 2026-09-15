using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Copilots;
using SufiChain.SufiPlatform.SufiCom.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Templates;

public class SufiComTemplateEditorCopilotSeedTests
{
    [Fact]
    public void Template_Editor_Seed_Should_Require_Current_Context_In_All_Cultures()
    {
        SufiComTemplateEditorCopilotKeys.EntityVersion.ShouldBe(5);

        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var prompt = SufiComTemplateEditorCopilotSeedTexts.Texts.SystemPrompt[culture];

            prompt.ShouldContain("copilotContext");
            prompt.ShouldContain("channel");
            prompt.ShouldContain("culture");
            prompt.ShouldContain("subject");
            prompt.ShouldContain("body");
            prompt.ShouldContain("placeholders");
        }
    }
}

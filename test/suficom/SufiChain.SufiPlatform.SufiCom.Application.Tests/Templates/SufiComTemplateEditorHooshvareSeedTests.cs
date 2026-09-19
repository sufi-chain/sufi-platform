using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Hooshvare;
using SufiChain.SufiPlatform.SufiCom.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Templates;

public class SufiComTemplateEditorHooshvareSeedTests
{
    [Fact]
    public void Template_Editor_Seed_Should_Require_Current_Context_In_All_Cultures()
    {
        SufiComTemplateEditorHooshvareKeys.EntityVersion.ShouldBe(0);

        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var prompt = SufiComTemplateEditorHooshvareSeedTexts.Texts.SystemPrompt[culture];

            prompt.ShouldContain("hooshvareContext");
            prompt.ShouldContain("channel");
            prompt.ShouldContain("culture");
            prompt.ShouldContain("subject");
            prompt.ShouldContain("body");
            prompt.ShouldContain("placeholders");
        }
    }
}

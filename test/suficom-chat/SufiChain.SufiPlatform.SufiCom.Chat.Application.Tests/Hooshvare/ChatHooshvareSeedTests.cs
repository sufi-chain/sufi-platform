using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Hooshvare;
using SufiChain.SufiPlatform.SufiCom.Chat.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Hooshvare;

public class ChatHooshvareSeedTests
{
    [Fact]
    public void Public_Assistant_Should_Be_General_Purpose_Without_Support_Shortcuts()
    {
        ChatHooshvareKeys.EntityVersion.ShouldBe(0);
        ChatHooshvareKeys.PublicAssistant.ShortcutIds.ShouldBeEmpty();
        ChatHooshvareSeedTexts.PublicAssistant.Texts.Shortcuts.ShouldBeEmpty();
        ChatHooshvareSeedTexts.PublicAssistant.Texts.DisplayName["en"].ShouldBe("Everyday Assistant");
        ChatHooshvareSeedTexts.PublicAssistant.Texts.DisplayName["fa"].ShouldBe("هوشواره روزمره");
    }

    [Fact]
    public void Chat_Seeds_Should_Keep_Their_Own_Context_And_Safety_Rules()
    {
        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var operatorPrompt = ChatHooshvareSeedTexts.OperatorReply.Texts.SystemPrompt[culture];
            operatorPrompt.ShouldContain("hooshvareContext");
            operatorPrompt.ShouldContain("conversation");
            operatorPrompt.ShouldContain("customer");
            operatorPrompt.ShouldContain("channel");

            var publicPrompt = ChatHooshvareSeedTexts.PublicAssistant.Texts.SystemPrompt[culture];
            publicPrompt.ShouldNotContain("hooshvareContext");
            publicPrompt.ShouldNotContain("customer-facing");
        }

        ChatHooshvareSeedTexts.PublicAssistant.Texts.SystemPrompt["en"]
            .ShouldContain("general-purpose AI assistant");
        ChatHooshvareSeedTexts.PublicAssistant.Texts.SystemPrompt["en"]
            .ShouldContain("only when the runtime supplies sources");
        ChatHooshvareSeedTexts.PublicAssistant.Texts.SystemPrompt["en"]
            .ShouldContain("untrusted evidence");
        new SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare.HooshvareRuntimeOptions()
            .UseWebSearch.ShouldBeFalse();
    }
}

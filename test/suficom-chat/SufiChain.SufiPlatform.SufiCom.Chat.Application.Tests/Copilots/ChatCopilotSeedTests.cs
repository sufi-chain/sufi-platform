using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Copilots;
using SufiChain.SufiPlatform.SufiCom.Chat.Data;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Copilots;

public class ChatCopilotSeedTests
{
    [Fact]
    public void Public_Assistant_Should_Be_General_Purpose_Without_Support_Shortcuts()
    {
        ChatCopilotKeys.EntityVersion.ShouldBe(8);
        ChatCopilotKeys.PublicAssistant.ShortcutIds.ShouldBeEmpty();
        ChatCopilotSeedTexts.PublicAssistant.Texts.Shortcuts.ShouldBeEmpty();
        ChatCopilotSeedTexts.PublicAssistant.Texts.DisplayName["en"].ShouldBe("Everyday Assistant");
        ChatCopilotSeedTexts.PublicAssistant.Texts.DisplayName["fa"].ShouldBe("هوشواره روزمره");
    }

    [Fact]
    public void Chat_Seeds_Should_Keep_Their_Own_Context_And_Safety_Rules()
    {
        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var operatorPrompt = ChatCopilotSeedTexts.OperatorReply.Texts.SystemPrompt[culture];
            operatorPrompt.ShouldContain("copilotContext");
            operatorPrompt.ShouldContain("conversation");
            operatorPrompt.ShouldContain("customer");
            operatorPrompt.ShouldContain("channel");

            var publicPrompt = ChatCopilotSeedTexts.PublicAssistant.Texts.SystemPrompt[culture];
            publicPrompt.ShouldNotContain("copilotContext");
            publicPrompt.ShouldNotContain("customer-facing");
        }

        ChatCopilotSeedTexts.PublicAssistant.Texts.SystemPrompt["en"]
            .ShouldContain("general-purpose AI assistant");
        ChatCopilotSeedTexts.PublicAssistant.Texts.SystemPrompt["en"]
            .ShouldContain("only when the runtime supplies sources");
        ChatCopilotSeedTexts.PublicAssistant.Texts.SystemPrompt["en"]
            .ShouldContain("untrusted evidence");
        new SufiChain.SufiPlatform.SufiAI.Copilots.Copilots.CopilotRuntimeOptions()
            .UseWebSearch.ShouldBeFalse();
    }
}

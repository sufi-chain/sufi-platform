using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Copilots;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using SufiChain.SufiPlatform.Data;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk;

public class HelpDeskAiWorkspacePurposeCopilotDefaultsTests
{
    [Theory]
    [InlineData(HelpDeskAiWorkspacePurpose.Default, PlatformCopilotKeys.HelpDeskVisitorSupport)]
    [InlineData(HelpDeskAiWorkspacePurpose.RagIndexing, PlatformCopilotKeys.HelpDeskKbArticleEditor)]
    [InlineData(HelpDeskAiWorkspacePurpose.LiveChat, PlatformCopilotKeys.HelpDeskVisitorSupport)]
    [InlineData(HelpDeskAiWorkspacePurpose.Ticketing, PlatformCopilotKeys.HelpDeskAgentReply)]
    [InlineData(HelpDeskAiWorkspacePurpose.Summarization, PlatformCopilotKeys.HelpDeskAgentReply)]
    [InlineData(HelpDeskAiWorkspacePurpose.ContentEditing, PlatformCopilotKeys.HelpDeskKbArticleEditor)]
    public void Should_Map_Purpose_To_Expected_Copilot_Key(HelpDeskAiWorkspacePurpose purpose, string expectedKey)
    {
        HelpDeskAiWorkspacePurposeCopilotDefaults.GetDefaultCopilotKey(purpose).ShouldBe(expectedKey);
    }

    [Fact]
    public void Should_Include_All_Purposes()
    {
        HelpDeskAiWorkspacePurposeCopilotDefaults.AllPurposes.Count.ShouldBe(6);
        foreach (var purpose in Enum.GetValues<HelpDeskAiWorkspacePurpose>())
        {
            HelpDeskAiWorkspacePurposeCopilotDefaults.AllPurposes.ShouldContain(purpose);
        }
    }
}

public class PlatformCopilotBusinessLocalizationKeysTests
{
    [Fact]
    public void Should_Use_Stable_Key_Based_Localization_Keys()
    {
        const string copilotKey = PlatformCopilotKeys.ChatPublicAssistant;

        BusinessLocalizationKeys.CopilotDisplayName(copilotKey)
            .ShouldBe("Copilot:Chat:PublicAssistant:DisplayName");
        BusinessLocalizationKeys.CopilotSystemPrompt(copilotKey)
            .ShouldBe("Copilot:Chat:PublicAssistant:SystemPrompt");
        BusinessLocalizationKeys.CopilotShortcut(copilotKey, "AskHours")
            .ShouldBe("Copilot:Chat:PublicAssistant:Shortcut:AskHours");
    }

    [Fact]
    public void Platform_Copilot_Keys_Should_Match_Module_Constants()
    {
        HelpDeskAgentReplyCopilotKeys.Key.ShouldBe(PlatformCopilotKeys.HelpDeskAgentReply);
        HelpDeskTicketTriageCopilotKeys.Key.ShouldBe(PlatformCopilotKeys.HelpDeskTicketTriage);
    }

    [Fact]
    public void AgentReply_Should_Include_Summarize_Shortcut_And_Entity_Version_2()
    {
        HelpDeskAgentReplyCopilotKeys.EntityVersion.ShouldBe(2);
        HelpDeskAgentReplyCopilotKeys.ShortcutIds.ShouldContain("Summarize");
    }

    [Fact]
    public void Hidden_Catalog_Keys_Should_Include_Coach_And_Retired_OperatorAssist()
    {
        PlatformCopilotKeys.IsHiddenFromAdminCatalog(PlatformCopilotKeys.CopilotCoach).ShouldBeTrue();
        PlatformCopilotKeys.IsHiddenFromAdminCatalog(PlatformCopilotKeys.HelpDeskOperatorAssist).ShouldBeTrue();
        PlatformCopilotKeys.IsHiddenFromAdminCatalog(PlatformCopilotKeys.HelpDeskAgentReply).ShouldBeFalse();
    }
}

using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Data;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk;

public class HelpDeskAiWorkspacePurposeHooshvareDefaultsTests
{
    [Theory]
    [InlineData(HelpDeskAiWorkspacePurpose.Default, PlatformHooshvareKeys.HelpDeskVisitorSupport)]
    [InlineData(HelpDeskAiWorkspacePurpose.RagIndexing, PlatformHooshvareKeys.HelpDeskKbArticleEditor)]
    [InlineData(HelpDeskAiWorkspacePurpose.LiveChat, PlatformHooshvareKeys.HelpDeskVisitorSupport)]
    [InlineData(HelpDeskAiWorkspacePurpose.Ticketing, PlatformHooshvareKeys.HelpDeskAgentReply)]
    [InlineData(HelpDeskAiWorkspacePurpose.Summarization, PlatformHooshvareKeys.HelpDeskAgentReply)]
    [InlineData(HelpDeskAiWorkspacePurpose.ContentEditing, PlatformHooshvareKeys.HelpDeskKbArticleEditor)]
    public void Should_Map_Purpose_To_Expected_Hooshvare_Key(HelpDeskAiWorkspacePurpose purpose, string expectedKey)
    {
        HelpDeskAiWorkspacePurposeHooshvareDefaults.GetDefaultHooshvareKey(purpose).ShouldBe(expectedKey);
    }

    [Fact]
    public void Should_Include_All_Purposes()
    {
        HelpDeskAiWorkspacePurposeHooshvareDefaults.AllPurposes.Count.ShouldBe(6);
        foreach (var purpose in Enum.GetValues<HelpDeskAiWorkspacePurpose>())
        {
            HelpDeskAiWorkspacePurposeHooshvareDefaults.AllPurposes.ShouldContain(purpose);
        }
    }
}

public class PlatformHooshvareBusinessLocalizationKeysTests
{
    [Fact]
    public void Should_Use_Stable_Key_Based_Localization_Keys()
    {
        const string hooshvareKey = PlatformHooshvareKeys.ChatPublicAssistant;

        BusinessLocalizationKeys.HooshvareDisplayName(hooshvareKey)
            .ShouldBe("Hooshvare:Chat:PublicAssistant:DisplayName");
        BusinessLocalizationKeys.HooshvareSystemPrompt(hooshvareKey)
            .ShouldBe("Hooshvare:Chat:PublicAssistant:SystemPrompt");
        BusinessLocalizationKeys.HooshvareShortcut(hooshvareKey, "AskHours")
            .ShouldBe("Hooshvare:Chat:PublicAssistant:Shortcut:AskHours");
    }

    [Fact]
    public void Platform_Hooshvare_Keys_Should_Match_Module_Constants()
    {
        HelpDeskAgentReplyHooshvareKeys.Key.ShouldBe(PlatformHooshvareKeys.HelpDeskAgentReply);
        HelpDeskTicketTriageHooshvareKeys.Key.ShouldBe(PlatformHooshvareKeys.HelpDeskTicketTriage);
    }

    [Fact]
    public void AgentReply_Should_Include_Summarize_Shortcut_And_Entity_Version_2()
    {
        HelpDeskAgentReplyHooshvareKeys.EntityVersion.ShouldBe(0);
        HelpDeskAgentReplyHooshvareKeys.ShortcutIds.ShouldContain("Summarize");
    }

    [Fact]
    public void Hidden_Catalog_Keys_Should_Include_Coach_And_Retired_OperatorAssist()
    {
        PlatformHooshvareKeys.IsHiddenFromAdminCatalog(PlatformHooshvareKeys.HooshvareCoach).ShouldBeTrue();
        PlatformHooshvareKeys.IsHiddenFromAdminCatalog(PlatformHooshvareKeys.HelpDeskOperatorAssist).ShouldBeTrue();
        PlatformHooshvareKeys.IsHiddenFromAdminCatalog(PlatformHooshvareKeys.HelpDeskAgentReply).ShouldBeFalse();
    }
}

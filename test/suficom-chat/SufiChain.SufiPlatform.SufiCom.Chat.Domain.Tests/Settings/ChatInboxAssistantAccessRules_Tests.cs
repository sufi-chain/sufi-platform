using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat.Copilots;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.Settings;

public class ChatInboxAssistantAccessRules_Tests
{
    [Fact]
    public void Parse_Should_Convert_Legacy_Keys_To_Public_Rules()
    {
        var rules = ChatInboxAssistantAccessRules.Parse(
            null,
            new[]
            {
                ChatCopilotKeys.PublicAssistant.Key,
                "Module:OtherAssistant"
            });

        rules.Count.ShouldBe(2);
        rules.ShouldAllBe(rule => rule.RoleIds.Count == 0);
        rules.Select(rule => rule.Key).ShouldBe(new[]
        {
            ChatCopilotKeys.PublicAssistant.Key,
            "Module:OtherAssistant"
        });
    }

    [Fact]
    public void Serialize_And_Parse_Should_Normalize_Rules()
    {
        var roleAId = Guid.NewGuid();
        var roleBId = Guid.NewGuid();
        var json = ChatInboxAssistantAccessRules.Serialize(new[]
        {
            new ChatInboxAssistantAccessRule
            {
                Key = $" {ChatCopilotKeys.PublicAssistant.Key} ",
                RoleIds = new List<Guid> { roleAId, roleAId, Guid.Empty }
            },
            new ChatInboxAssistantAccessRule
            {
                Key = ChatCopilotKeys.PublicAssistant.Key,
                RoleIds = new List<Guid> { roleBId }
            }
        });

        var rules = ChatInboxAssistantAccessRules.Parse(json);

        rules.Count.ShouldBe(1);
        rules[0].Key.ShouldBe(ChatCopilotKeys.PublicAssistant.Key);
        rules[0].RoleIds.ShouldBe(new[] { roleAId, roleBId });
    }

    [Fact]
    public void Empty_Role_List_Should_Remain_Public()
    {
        var json = ChatInboxAssistantAccessRules.Serialize(new[]
        {
            new ChatInboxAssistantAccessRule
            {
                Key = ChatCopilotKeys.PublicAssistant.Key
            }
        });

        var rules = ChatInboxAssistantAccessRules.Parse(json);

        rules.Single().RoleIds.ShouldBeEmpty();
    }

    [Fact]
    public void Empty_Rule_List_Should_Remain_Empty()
    {
        var json = ChatInboxAssistantAccessRules.Serialize(
            Array.Empty<ChatInboxAssistantAccessRule>());

        ChatInboxAssistantAccessRules.Parse(json).ShouldBeEmpty();
    }

    [Fact]
    public void Invalid_Json_Should_Fall_Back_To_Legacy_Keys()
    {
        var rules = ChatInboxAssistantAccessRules.Parse(
            "not-json",
            new[] { "Legacy:Assistant" });

        rules.Single().Key.ShouldBe("Legacy:Assistant");
        rules.Single().RoleIds.ShouldBeEmpty();
    }
}

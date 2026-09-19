using Shouldly;

using SufiChain.SufiPlatform.SufiCom.Chat.Hooshvare;

using Xunit;



namespace SufiChain.SufiPlatform.SufiCom.Chat.Settings;



public class ChatInboxAssistantKeys_Tests

{

    [Fact]

    public void Parse_Should_Return_Default_When_Value_Is_Empty()

    {

        ChatInboxAssistantKeys.Parse(null).ShouldBe(ChatInboxAssistantKeys.DefaultKeys);

        ChatInboxAssistantKeys.Parse("").ShouldBe(ChatInboxAssistantKeys.DefaultKeys);

        ChatInboxAssistantKeys.Parse("   ").ShouldBe(ChatInboxAssistantKeys.DefaultKeys);

    }



    [Fact]

    public void Parse_Should_Return_Default_When_Json_Is_Invalid_Or_Empty_Array()

    {

        ChatInboxAssistantKeys.Parse("not-json").ShouldBe(ChatInboxAssistantKeys.DefaultKeys);

        ChatInboxAssistantKeys.Parse("[]").ShouldBe(ChatInboxAssistantKeys.DefaultKeys);

    }



    [Fact]

    public void Parse_Should_Normalize_Keys()

    {

        var keys = ChatInboxAssistantKeys.Parse(

            "[\" SufiComChat:PublicAssistant \", \"Other:Assistant\", \"Other:Assistant\", \"\"]");



        keys.Count.ShouldBe(2);

        keys.ShouldContain(ChatHooshvareKeys.PublicAssistant.Key);

        keys.ShouldContain("Other:Assistant");

    }



    [Fact]

    public void Serialize_Should_Fall_Back_To_Default_When_Empty()

    {

        ChatInboxAssistantKeys.Serialize(null).ShouldBe(ChatInboxAssistantKeys.DefaultJson);

        ChatInboxAssistantKeys.Serialize(Array.Empty<string>()).ShouldBe(ChatInboxAssistantKeys.DefaultJson);

    }



    [Fact]

    public void SerializeConfigured_Should_Preserve_Empty_Allowlist()

    {

        ChatInboxAssistantKeys.SerializeConfigured(Array.Empty<string>()).ShouldBe("[]");

    }



    [Fact]

    public void Serialize_And_Parse_Should_RoundTrip()

    {

        var json = ChatInboxAssistantKeys.Serialize(new[]

        {

            ChatHooshvareKeys.PublicAssistant.Key,

            "Module:OtherAssistant"

        });



        var keys = ChatInboxAssistantKeys.Parse(json);



        keys.ShouldBe(new[]

        {

            ChatHooshvareKeys.PublicAssistant.Key,

            "Module:OtherAssistant"

        });

    }

}


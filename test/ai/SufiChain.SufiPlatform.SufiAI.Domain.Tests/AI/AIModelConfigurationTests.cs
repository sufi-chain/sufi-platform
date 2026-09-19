using System;
using Shouldly;
using Volo.Abp;
using Xunit;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class AIModelConfigurationTests
{
    [Fact]
    public void Should_Default_Protocol_Window_And_Selectability()
    {
        var configuration = new AIModelConfiguration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.ChatCompletion,
            "gpt-4o");

        configuration.OpenAIApiMode.ShouldBe(OpenAIApiMode.ChatCompletions);
        configuration.MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
        configuration.IsUserSelectable.ShouldBeFalse();
        configuration.DisplayName.ShouldBeNull();
        configuration.Description.ShouldBeNull();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Should_Reject_Negative_Max_Context_Tokens(int maxContextTokens)
    {
        var configuration = new AIModelConfiguration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.ChatCompletion,
            "gpt-4o");

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            configuration.UpdateConfiguration(
                "gpt-4o",
                apiEndpoint: null,
                apiKey: null,
                priority: 0,
                maxContextTokens: maxContextTokens));

        exception.Code.ShouldBe(AIErrorCodes.InvalidMaxContextTokens);
    }

    [Fact]
    public void Should_Normalize_Legacy_Zero_Max_Context_Tokens_To_Default()
    {
        var configuration = new AIModelConfiguration(
            Guid.NewGuid(), Guid.NewGuid(), AICapabilityType.ChatCompletion, "gpt-4o");
        configuration.UpdateConfiguration("gpt-4o", null, null, 0, maxContextTokens: 128000);
        configuration.UpdateConfiguration("gpt-4o", null, null, 0, maxContextTokens: 0);

        configuration.MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
    }

    [Fact]
    public void Should_Normalize_Optional_Display_Fields()
    {
        var configuration = new AIModelConfiguration(
            Guid.NewGuid(),
            Guid.NewGuid(),
            AICapabilityType.ChatCompletion,
            "gpt-4o");

        configuration.UpdateConfiguration(
            "gpt-4o",
            apiEndpoint: null,
            apiKey: null,
            priority: 0,
            displayName: "  Fast chat  ",
            isUserSelectable: true,
            description: "  Shown in the picker  ",
            maxContextTokens: 128000);

        configuration.DisplayName.ShouldBe("Fast chat");
        configuration.Description.ShouldBe("Shown in the picker");
        configuration.IsUserSelectable.ShouldBeTrue();
        configuration.MaxContextTokens.ShouldBe(128000);
        configuration.OpenAIApiMode.ShouldBe(OpenAIApiMode.ChatCompletions);
    }
}

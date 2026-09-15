using System;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Xunit;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.Workspaces;

public class WorkspaceTests : SufiAITestBase<SufiAIDomainTestModule>
{
    [Fact]
    public void Should_Create_Workspace_With_Valid_Data()
    {
        // Arrange & Act
        var workspace = new Workspace(
            AITestData.Workspaces.DefaultWorkspaceId,
            AITestData.Workspaces.DefaultWorkspaceName,
            AIProviderType.OpenAI,
            AITestData.Workspaces.DefaultModelId
        );

        // Assert
        workspace.Id.ShouldBe(AITestData.Workspaces.DefaultWorkspaceId);
        workspace.Name.ShouldBe(AITestData.Workspaces.DefaultWorkspaceName);
        workspace.Provider.ShouldBe(AIProviderType.OpenAI);
        workspace.Model.ShouldBe(AITestData.Workspaces.DefaultModelId);
        workspace.IsActive.ShouldBeTrue();
    }

    [Fact]
    public void Should_Update_Workspace_Configuration()
    {
        // Arrange
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-3.5-turbo"
        );

        // Act
        workspace.UpdateConfiguration(
            "gpt-4",
            "new-api-key",
            "https://custom.openai.com/v1"
        );

        // Assert
        workspace.Model.ShouldBe("gpt-4");
        workspace.ApiKey.ShouldBe("new-api-key");
        workspace.ApiBaseUrl.ShouldBe("https://custom.openai.com/v1");
    }

    [Fact]
    public void Should_Activate_And_Deactivate_Workspace()
    {
        // Arrange
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-4"
        );

        // Act & Assert - Deactivate
        workspace.Deactivate();
        workspace.IsActive.ShouldBeFalse();

        // Act & Assert - Activate
        workspace.Activate();
        workspace.IsActive.ShouldBeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_Exception_For_Invalid_Name(string? invalidName)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
        {
            new Workspace(
                Guid.NewGuid(),
                invalidName!,
                AIProviderType.OpenAI,
                "gpt-4"
            );
        });
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void Should_Throw_Exception_For_Invalid_ModelId(string? invalidModelId)
    {
        // Act & Assert
        Should.Throw<ArgumentException>(() =>
        {
            new Workspace(
                Guid.NewGuid(),
                "test",
                AIProviderType.OpenAI,
                invalidModelId!
            );
        });
    }

    [Fact]
    public void Should_Keep_Primary_Chat_Route_Price_When_Updating_Workspace()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-3.5-turbo"
        );
        var configuration = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "gpt-3.5-turbo",
            inputCostPer1MTokens: 1.5m,
            outputCostPer1MTokens: 2.5m);

        workspace.UpdateConfiguration(
            "gpt-4",
            "key",
            "https://workspace.example/v1",
            inputCostPer1MTokens: 9m,
            outputCostPer1MTokens: 11m);
        workspace.UpdatePrimaryChatConfiguration(
            "gpt-4",
            "https://workspace.example/v1");

        configuration.ModelId.ShouldBe("gpt-4");
        configuration.ApiEndpoint.ShouldBe("https://workspace.example/v1");
        configuration.InputCostPer1MTokens.ShouldBe(1.5m);
        configuration.OutputCostPer1MTokens.ShouldBe(2.5m);
        configuration.MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
    }

    [Fact]
    public void Should_Treat_Zero_Max_Context_Tokens_As_Default()
    {
        var workspace = new Workspace(
            Guid.NewGuid(),
            "test",
            AIProviderType.OpenAI,
            "gpt-4"
        );
        var configuration = workspace.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "gpt-4");

        configuration.UpdateConfiguration(
            "gpt-4",
            apiEndpoint: null,
            apiKey: null,
            priority: 0,
            maxContextTokens: 0);

        configuration.MaxContextTokens.ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
        AIModelConfiguration.NormalizeMaxContextTokens(0).ShouldBe(AIModelConfiguration.DefaultMaxContextTokens);
    }
}

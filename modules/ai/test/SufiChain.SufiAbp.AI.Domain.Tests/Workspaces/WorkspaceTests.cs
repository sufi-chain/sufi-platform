using System;
using Shouldly;
using SufiChain.SufiAbp.AI.Workspaces;
using Xunit;
using SufiChain.SufiAbp.AI;

namespace SufiChain.SufiAbp.AI.Domain.Tests.Workspaces;

public class WorkspaceTests : AITestBase<AIDomainTestModule>
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
            "https://custom.openai.com/v1",
            null,
            0.7f,
            2000
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
}

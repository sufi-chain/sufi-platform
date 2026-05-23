using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiAbp.AIManagement.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiAbp.AIManagement.Domain.Tests.Workspaces;

public class WorkspaceManagerTests : AIManagementTestBase<AIManagementDomainTestModule>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly WorkspaceManager _workspaceManager;

    public WorkspaceManagerTests()
    {
        _workspaceRepository = Substitute.For<IWorkspaceRepository>();
        _workspaceManager = new WorkspaceManager(_workspaceRepository);
    }

    [Fact]
    public async Task Should_Create_Workspace_With_Unique_Name()
    {
        // Arrange
        _workspaceRepository
            .FindByNameAsync(AIManagementTestData.Workspaces.DefaultWorkspaceName)
            .Returns(Task.FromResult<Workspace?>(null));

        // Act
        var workspace = await _workspaceManager.CreateAsync(
            AIManagementTestData.Workspaces.DefaultWorkspaceName,
            AIProviderType.OpenAI,
            AIManagementTestData.Workspaces.DefaultModelId
        );

        // Assert
        workspace.ShouldNotBeNull();
        workspace.Name.ShouldBe(AIManagementTestData.Workspaces.DefaultWorkspaceName);
    }

    [Fact]
    public async Task Should_Throw_Exception_For_Duplicate_Workspace_Name()
    {
        // Arrange
        var existingWorkspace = new Workspace(
            Guid.NewGuid(),
            AIManagementTestData.Workspaces.DefaultWorkspaceName,
            AIProviderType.OpenAI,
            "gpt-4"
        );

        _workspaceRepository
            .FindByNameAsync(AIManagementTestData.Workspaces.DefaultWorkspaceName)
            .Returns(Task.FromResult<Workspace?>(existingWorkspace));

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _workspaceManager.CreateAsync(
                AIManagementTestData.Workspaces.DefaultWorkspaceName,
                AIProviderType.OpenAI,
                "gpt-4"
            );
        });
    }

    [Fact]
    public async Task Should_Change_Workspace_Name_If_Unique()
    {
        // Arrange
        var workspace = new Workspace(
            Guid.NewGuid(),
            "old-name",
            AIProviderType.OpenAI,
            "gpt-4"
        );

        _workspaceRepository
            .FindByNameAsync("new-name")
            .Returns(Task.FromResult<Workspace?>(null));

        // Act
        await _workspaceManager.ChangeNameAsync(workspace, "new-name");

        // Assert
        workspace.Name.ShouldBe("new-name");
    }

    [Fact]
    public async Task Should_Throw_Exception_When_Changing_To_Duplicate_Name()
    {
        // Arrange
        var workspace = new Workspace(
            Guid.NewGuid(),
            "old-name",
            AIProviderType.OpenAI,
            "gpt-4"
        );

        var existingWorkspace = new Workspace(
            Guid.NewGuid(),
            "new-name",
            AIProviderType.OpenAI,
            "gpt-4"
        );

        _workspaceRepository
            .FindByNameAsync("new-name")
            .Returns(Task.FromResult<Workspace?>(existingWorkspace));

        // Act & Assert
        await Should.ThrowAsync<BusinessException>(async () =>
        {
            await _workspaceManager.ChangeNameAsync(workspace, "new-name");
        });
    }
}

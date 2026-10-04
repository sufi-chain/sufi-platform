using System;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.Workspaces;

public class WorkspaceManagerTests : SufiAITestBase<SufiAIDomainTestModule>
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
            .FindByNameAsync(AITestData.Workspaces.DefaultWorkspaceName)
            .Returns(Task.FromResult<Workspace?>(null));

        // Act
        await _workspaceManager.ValidateNameAsync(
            AITestData.Workspaces.DefaultWorkspaceName
        );

        var workspace = new Workspace(
            Guid.NewGuid(),
            AITestData.Workspaces.DefaultWorkspaceName,
            AIProviderType.OpenAI,
            AITestData.Workspaces.DefaultModelId
        );

        // Assert
        workspace.ShouldNotBeNull();
        workspace.Name.ShouldBe(AITestData.Workspaces.DefaultWorkspaceName);
    }

    [Fact]
    public async Task Should_Throw_Exception_For_Duplicate_Workspace_Name()
    {
        // Arrange
        var existingWorkspace = new Workspace(
            Guid.NewGuid(),
            AITestData.Workspaces.DefaultWorkspaceName,
            AIProviderType.OpenAI,
            "gpt-4"
        );

        _workspaceRepository
            .FindByNameAsync(AITestData.Workspaces.DefaultWorkspaceName)
            .Returns(Task.FromResult<Workspace?>(existingWorkspace));

        // Act & Assert
        await Should.ThrowAsync<global::Volo.Abp.BusinessException>(async () =>
        {
            await _workspaceManager.ValidateNameAsync(
                AITestData.Workspaces.DefaultWorkspaceName
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
        await _workspaceManager.ValidateNameAsync("new-name", workspace.Id);
        workspace.SetName("new-name");

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
        await Should.ThrowAsync<global::Volo.Abp.BusinessException>(async () =>
        {
            await _workspaceManager.ValidateNameAsync("new-name", workspace.Id);
        });
    }

    [Fact]
    public void CreateCopy_preserves_capability_flags_and_user_selectable()
    {
        var source = new Workspace(Guid.NewGuid(), "host-workspace", AIProviderType.OpenRouter, "openrouter/free");
        source.UpdateConfiguration(
            "openrouter/free",
            "sk-host",
            "https://or-gateway.sufichain.com/v1");
        var chat = source.AddModelConfiguration(
            AICapabilityType.ChatCompletion,
            "openrouter/free",
            apiEndpoint: "https://or-gateway.sufichain.com/v1",
            priority: 0,
            displayName: "Free",
            isUserSelectable: true);
        chat.SetChatCapabilities(
            acceptsImageInput: true,
            acceptsFileInput: true,
            supportsReasoning: true,
            reasoningEfforts: "low,medium,high",
            defaultReasoningEffort: "medium",
            capabilitySource: ModelCapabilitySource.LiveCatalog);
        source.AddModelConfiguration(
            AICapabilityType.AudioTranscription,
            "nemotron-asr",
            priority: 1,
            isUserSelectable: false);

        var clone = _workspaceManager.CreateCopy(source, "tenant-copy", Guid.NewGuid(), Guid.NewGuid());

        var copiedChat = clone.ModelConfigurations.Single(item => item.ModelId == "openrouter/free");
        copiedChat.Id.ShouldNotBe(chat.Id);
        copiedChat.IsUserSelectable.ShouldBeTrue();
        copiedChat.AcceptsImageInput.ShouldBe(true);
        copiedChat.AcceptsFileInput.ShouldBe(true);
        copiedChat.SupportsReasoning.ShouldBe(true);
        copiedChat.ReasoningEfforts.ShouldBe("low,medium,high");
        copiedChat.DefaultReasoningEffort.ShouldBe("medium");
        copiedChat.CapabilitySource.ShouldBe(ModelCapabilitySource.LiveCatalog);

        var copiedAudio = clone.ModelConfigurations.Single(item => item.CapabilityType == AICapabilityType.AudioTranscription);
        copiedAudio.IsUserSelectable.ShouldBeFalse();
        copiedAudio.AcceptsImageInput.ShouldBeNull();
        copiedAudio.CapabilitySource.ShouldBeNull();
    }
}

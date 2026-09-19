using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Domain.Tests.AI;

public class AIModelRouteResolverTests
{
    [Fact]
    public void Should_Keep_Implicit_Primary_And_Chat_Default_Fallback()
    {
        var workspace = CreateWorkspace();
        workspace.UpdateConfiguration("fallback-chat", "sk-workspace", "https://api.example/v1");
        var primary = AddRoute(workspace, "primary-chat", isUserSelectable: false, priority: 0);
        AddRoute(workspace, "other-chat", isUserSelectable: true, priority: 10);

        var resolved = CreateResolver(workspace).Resolve(
            workspace,
            AICapabilityType.ChatCompletion,
            AIModelRouteSelection.Implicit);

        resolved.IsExplicitSelection.ShouldBeFalse();
        resolved.IsFallback.ShouldBeFalse();
        resolved.ModelConfigurationId.ShouldBe(primary.Id);
        resolved.ModelId.ShouldBe("primary-chat");
        resolved.IsConfigured.ShouldBeTrue();
    }

    [Fact]
    public void Should_Use_Workspace_Default_Model_When_No_Chat_Route_Exists()
    {
        var workspace = CreateWorkspace();
        workspace.UpdateConfiguration("workspace-default", "sk-workspace", "https://api.example/v1");

        var resolved = CreateResolver(workspace).Resolve(
            workspace,
            AICapabilityType.ChatCompletion,
            AIModelRouteSelection.Implicit);

        resolved.IsExplicitSelection.ShouldBeFalse();
        resolved.IsFallback.ShouldBeTrue();
        resolved.ModelConfigurationId.ShouldBeNull();
        resolved.ModelId.ShouldBe("workspace-default");
        resolved.IsConfigured.ShouldBeTrue();
    }

    [Fact]
    public void Should_Resolve_Explicit_Selectable_Route()
    {
        var workspace = CreateWorkspace();
        var selected = AddRoute(workspace, "selectable-chat", isUserSelectable: true, priority: 5);
        AddRoute(workspace, "primary-chat", isUserSelectable: false, priority: 0);

        var resolved = CreateResolver(workspace).Resolve(
            workspace,
            AICapabilityType.ChatCompletion,
            new AIModelRouteSelection { ModelConfigurationId = selected.Id });

        resolved.IsExplicitSelection.ShouldBeTrue();
        resolved.IsFallback.ShouldBeFalse();
        resolved.ModelConfigurationId.ShouldBe(selected.Id);
        resolved.ModelId.ShouldBe("selectable-chat");
    }

    [Fact]
    public void Should_Reject_Missing_Explicit_Route_Without_Fallback()
    {
        var workspace = CreateWorkspace();
        AddRoute(workspace, "primary-chat", isUserSelectable: false);

        var missingId = Guid.NewGuid();
        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace).Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection { ModelConfigurationId = missingId }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteNotFound);
        exception.Data["ModelConfigurationId"].ShouldBe(missingId);
    }

    [Fact]
    public async Task Should_Reject_Route_From_Another_Workspace()
    {
        var workspace = CreateWorkspace();
        var foreignWorkspaceId = Guid.NewGuid();
        var foreign = new AIModelConfiguration(
            Guid.NewGuid(),
            foreignWorkspaceId,
            AICapabilityType.ChatCompletion,
            "foreign-chat");
        foreign.UpdateConfiguration(
            "foreign-chat",
            "https://api.example/v1",
            "sk-foreign",
            0,
            isUserSelectable: true);

        var configurationRepository = Substitute.For<IAIModelConfigurationRepository>();
        configurationRepository
            .FindAsync(Arg.Is(foreign.Id), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(foreign);

        var exception = await Should.ThrowAsync<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace, configurationRepository).ResolveAsync(
                workspace.Id,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection { ModelConfigurationId = foreign.Id }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteOutsideWorkspace);
        exception.Data["ActualWorkspaceId"].ShouldBe(foreignWorkspaceId);
    }

    [Fact]
    public void Should_Reject_Disabled_Explicit_Route()
    {
        var workspace = CreateWorkspace();
        var route = AddRoute(workspace, "disabled-chat", isUserSelectable: true);
        route.Disable();

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace).Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection { ModelConfigurationId = route.Id }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteNotSelectable);
        exception.Data["Rule"].ShouldBe(AIModelRouteResolver.DisabledRule);
    }

    [Fact]
    public void Should_Reject_Capability_Mismatch()
    {
        var workspace = CreateWorkspace();
        var embeddings = AddRoute(
            workspace,
            "embed-1",
            isUserSelectable: true,
            capabilityType: AICapabilityType.Embeddings);

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace).Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection { ModelConfigurationId = embeddings.Id }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteCapabilityMismatch);
    }

    [Fact]
    public void Should_Reject_Route_That_Is_Not_User_Selectable()
    {
        var workspace = CreateWorkspace();
        var internalRoute = AddRoute(workspace, "internal-chat", isUserSelectable: false);

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace).Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection { ModelConfigurationId = internalRoute.Id }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteNotSelectable);
        exception.Data["Rule"].ShouldBe(AIModelRouteResolver.NotUserSelectableRule);
    }

    [Fact]
    public void Should_Reject_Explicit_Route_Outside_Allowlist()
    {
        var workspace = CreateWorkspace();
        var allowed = AddRoute(workspace, "allowed-chat", isUserSelectable: true, priority: 0);
        var blocked = AddRoute(workspace, "blocked-chat", isUserSelectable: true, priority: 1);

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace).Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection
                {
                    ModelConfigurationId = blocked.Id,
                    AllowedModelConfigurationIds = new[] { allowed.Id }
                }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteNotAllowedForHooshvare);
    }

    [Fact]
    public void Should_Allow_Empty_Allowlist_For_Any_Selectable_Route()
    {
        var workspace = CreateWorkspace();
        var selected = AddRoute(workspace, "selectable-chat", isUserSelectable: true);

        var resolved = CreateResolver(workspace).Resolve(
            workspace,
            AICapabilityType.ChatCompletion,
            new AIModelRouteSelection
            {
                ModelConfigurationId = selected.Id,
                AllowedModelConfigurationIds = Array.Empty<Guid>()
            });

        resolved.ModelConfigurationId.ShouldBe(selected.Id);
        resolved.IsExplicitSelection.ShouldBeTrue();
    }

    [Fact]
    public void Should_Reject_Responses_Route_When_Tools_Are_Required()
    {
        var workspace = CreateWorkspace();
        var responses = AddRoute(
            workspace,
            "responses-chat",
            isUserSelectable: true,
            openAIApiMode: OpenAIApiMode.Responses);

        var exception = Should.Throw<global::Volo.Abp.BusinessException>(() =>
            CreateResolver(workspace).Resolve(
                workspace,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection
                {
                    ModelConfigurationId = responses.Id,
                    RequiresToolCalling = true
                }));

        exception.Code.ShouldBe(AIErrorCodes.ModelRouteRequiresChatCompletions);
    }

    [Fact]
    public void Should_List_Selectable_Routes_Using_Live_Configured_Snapshot()
    {
        var workspace = CreateWorkspace();
        workspace.UpdateConfiguration("workspace-default", "sk-workspace", "https://api.example/v1");
        var selectable = AddRoute(workspace, "shown-chat", isUserSelectable: true, priority: 1);
        AddRoute(workspace, "hidden-chat", isUserSelectable: false, priority: 0);
        var disabled = AddRoute(workspace, "disabled-chat", isUserSelectable: true, priority: 2);
        disabled.Disable();

        var listed = CreateResolver(workspace).ListSelectableRoutes(
            workspace,
            AICapabilityType.ChatCompletion);

        listed.Count.ShouldBe(1);
        listed[0].ModelConfigurationId.ShouldBe(selectable.Id);
        listed[0].IsConfigured.ShouldBeTrue();
        listed[0].IsExplicitSelection.ShouldBeFalse();
    }

    private static AIModelRouteResolver CreateResolver(
        Workspace workspace,
        IAIModelConfigurationRepository? configurationRepository = null)
    {
        var workspaceRepository = Substitute.For<IWorkspaceRepository>();
        workspaceRepository
            .FindAsync(workspace.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(workspace);

        var provider = Substitute.For<IAIProvider>();
        provider.ProviderType.Returns(AIProviderType.OpenAI);
        provider.SupportsCapability(Arg.Any<AICapabilityType>()).Returns(true);

        var credentials = Substitute.For<IAICredentialResolver>();
        credentials.DecryptApiKey(Arg.Any<string?>()).Returns(call => call.Arg<string?>());

        var runtime = new WorkspaceRuntimeConfigurationResolver(
            workspaceRepository,
            new[] { provider },
            credentials);

        return new AIModelRouteResolver(
            workspaceRepository,
            configurationRepository ?? Substitute.For<IAIModelConfigurationRepository>(),
            runtime);
    }

    private static Workspace CreateWorkspace()
    {
        return new Workspace(
            Guid.NewGuid(),
            "route-tests",
            AIProviderType.OpenAI,
            "workspace-default");
    }

    private static AIModelConfiguration AddRoute(
        Workspace workspace,
        string modelId,
        bool isUserSelectable,
        int priority = 0,
        AICapabilityType capabilityType = AICapabilityType.ChatCompletion,
        OpenAIApiMode openAIApiMode = OpenAIApiMode.ChatCompletions)
    {
        return workspace.AddModelConfiguration(
            capabilityType,
            modelId,
            apiEndpoint: "https://api.example/v1",
            apiKey: "sk-route",
            priority: priority,
            openAIApiMode: openAIApiMode,
            isUserSelectable: isUserSelectable);
    }
}

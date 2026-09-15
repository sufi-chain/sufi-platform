using NSubstitute;
using Shouldly;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiPlatform.HelpDesk.Ai;
using SufiChain.SufiPlatform.HelpDesk.Options;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;
using Volo.Abp.Caching;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk;

public class HelpDeskAiWorkspaceResolverTests
{
    private readonly IProjectAiWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IDistributedCache<HelpDeskProjectAiWorkspaceAssignmentCacheItem> _cache;
    private readonly IHelpDeskOptionsProvider _optionsProvider;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICopilotDefinitionRepository _copilotDefinitionRepository;
    private readonly CopilotBusinessLocalizationService _businessLocalization;
    private readonly ICopilotWorkspaceResolver _copilotWorkspaceResolver;
    private readonly HelpDeskAiWorkspaceResolver _resolver;

    public HelpDeskAiWorkspaceResolverTests()
    {
        _assignmentRepository = Substitute.For<IProjectAiWorkspaceAssignmentRepository>();
        _cache = Substitute.For<IDistributedCache<HelpDeskProjectAiWorkspaceAssignmentCacheItem>>();
        _optionsProvider = Substitute.For<IHelpDeskOptionsProvider>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _copilotDefinitionRepository = Substitute.For<ICopilotDefinitionRepository>();
        _businessLocalization = new CopilotBusinessLocalizationService(Substitute.For<IStringLocalizerFactory>());
        _copilotWorkspaceResolver = Substitute.For<ICopilotWorkspaceResolver>();

        _cache
            .GetOrAddAsync(
                Arg.Any<string>(),
                Arg.Any<Func<Task<HelpDeskProjectAiWorkspaceAssignmentCacheItem>>>(),
                Arg.Any<Func<Microsoft.Extensions.Caching.Distributed.DistributedCacheEntryOptions>>(),
                Arg.Any<bool?>(),
                Arg.Any<bool>(),
                Arg.Any<CancellationToken>())
            .Returns(callInfo => callInfo.Arg<Func<Task<HelpDeskProjectAiWorkspaceAssignmentCacheItem>>>()());

        _resolver = new HelpDeskAiWorkspaceResolver(
            _assignmentRepository,
            _cache,
            _optionsProvider,
            _currentTenant,
            _copilotDefinitionRepository,
            _businessLocalization,
            new HelpDeskAiBindingEnricher(
                _copilotWorkspaceResolver,
                NullLogger<HelpDeskAiBindingEnricher>.Instance),
            NullLogger<HelpDeskAiWorkspaceResolver>.Instance);
    }

    [Fact]
    public async Task Should_Return_Purpose_Assignment_First()
    {
        var projectId = Guid.NewGuid();
        var defaultCopilotId = Guid.NewGuid();
        var contentCopilotId = Guid.NewGuid();
        var contentWorkspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId)
            .Returns(
            [
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.Default,
                    defaultCopilotId,
                    "Default Copilot"),
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.ContentEditing,
                    contentCopilotId,
                    "Content Copilot")
            ]);
        _copilotDefinitionRepository.GetAsync(contentCopilotId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateCopilot(contentCopilotId, "Content Copilot", contentWorkspaceId));
        _copilotWorkspaceResolver.ResolveAsync(contentWorkspaceId)
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = contentWorkspaceId,
                WorkspaceName = "content-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.Project);
        result.CopilotId.ShouldBe(contentCopilotId);
        result.CopilotName.ShouldBe("Content Copilot");
        result.WorkspaceId.ShouldBe(contentWorkspaceId);
        result.WorkspaceName.ShouldBe("content-workspace");
    }

    [Fact]
    public async Task Should_Fallback_To_Project_Default_Assignment()
    {
        var projectId = Guid.NewGuid();
        var defaultCopilotId = Guid.NewGuid();
        var defaultWorkspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId)
            .Returns(
            [
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.Default,
                    defaultCopilotId,
                    "Default Copilot")
            ]);
        _copilotDefinitionRepository.GetAsync(defaultCopilotId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateCopilot(defaultCopilotId, "Default Copilot", defaultWorkspaceId));
        _copilotWorkspaceResolver.ResolveAsync(defaultWorkspaceId)
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = defaultWorkspaceId,
                WorkspaceName = "default-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.ProjectDefault);
        result.CopilotId.ShouldBe(defaultCopilotId);
        result.CopilotName.ShouldBe("Default Copilot");
        result.WorkspaceId.ShouldBe(defaultWorkspaceId);
        result.WorkspaceName.ShouldBe("default-workspace");
    }

    [Fact]
    public async Task Should_Fallback_To_Tenant_Default_Copilot_When_Project_Assignment_Is_Missing()
    {
        var projectId = Guid.NewGuid();
        var tenantCopilotId = Guid.NewGuid();
        var tenantWorkspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId).Returns([]);
        _optionsProvider.GetAsync().Returns(new HelpDeskOptions { DefaultAiCopilotId = tenantCopilotId });
        _copilotDefinitionRepository.GetAsync(tenantCopilotId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateCopilot(tenantCopilotId, "Tenant Copilot", tenantWorkspaceId));
        _copilotWorkspaceResolver.ResolveAsync(tenantWorkspaceId)
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = tenantWorkspaceId,
                WorkspaceName = "tenant-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.TenantDefault);
        result.CopilotId.ShouldBe(tenantCopilotId);
        result.CopilotName.ShouldBe("Tenant Copilot");
        result.WorkspaceId.ShouldBe(tenantWorkspaceId);
        result.WorkspaceName.ShouldBe("tenant-workspace");
    }

    [Fact]
    public async Task Should_Return_MissingAssignment_When_No_Assignment_And_No_Default_Setting()
    {
        var projectId = Guid.NewGuid();
        _assignmentRepository.GetListByProjectAsync(projectId).Returns([]);
        _optionsProvider.GetAsync().Returns(new HelpDeskOptions { DefaultAiCopilotId = null });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeFalse();
        result.Readiness.ShouldBe(HelpDeskAiBindingReadiness.MissingAssignment);
        result.Source.ShouldBe(HelpDeskAiBindingSource.Missing);
    }

    [Fact]
    public async Task Should_Return_MissingWorkspace_When_Copilot_Has_No_Workspace()
    {
        var projectId = Guid.NewGuid();
        var copilotId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId)
            .Returns([
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.ContentEditing,
                    copilotId,
                    "Content Copilot")
            ]);
        _copilotDefinitionRepository.GetAsync(copilotId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateCopilot(copilotId, "Content Copilot", workspaceId));
        _copilotWorkspaceResolver.ResolveAsync(workspaceId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<CopilotWorkspaceBinding>(
                new InvalidOperationException("Workspace not found")));

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.Readiness.ShouldBe(HelpDeskAiBindingReadiness.MissingWorkspace);
        result.CopilotId.ShouldBe(copilotId);
        result.CopilotName.ShouldBe("Content Copilot");
    }

    [Fact]
    public async Task Should_Use_Tenant_Default_When_ProjectId_Is_Null()
    {
        var tenantCopilotId = Guid.NewGuid();
        var tenantWorkspaceId = Guid.NewGuid();
        _optionsProvider.GetAsync().Returns(new HelpDeskOptions { DefaultAiCopilotId = tenantCopilotId });
        _copilotDefinitionRepository.GetAsync(tenantCopilotId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateCopilot(tenantCopilotId, "Tenant Copilot", tenantWorkspaceId));
        _copilotWorkspaceResolver.ResolveAsync(tenantWorkspaceId)
            .Returns(new CopilotWorkspaceBinding
            {
                WorkspaceId = tenantWorkspaceId,
                WorkspaceName = "tenant-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(null, HelpDeskAiWorkspacePurpose.LiveChat);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.TenantDefault);
        result.CopilotName.ShouldBe("Tenant Copilot");
        result.WorkspaceId.ShouldBe(tenantWorkspaceId);
        result.WorkspaceName.ShouldBe("tenant-workspace");
        await _assignmentRepository.DidNotReceiveWithAnyArgs().GetListByProjectAsync(default);
    }

    private static CopilotDefinition CreateCopilot(Guid id, string displayName, Guid workspaceId)
    {
        return new CopilotDefinition(
            id,
            tenantId: null,
            sourceModule: "HelpDesk.LiveChat",
            displayName,
            CopilotKind.Copilot,
            HelpDeskAiWorkspacePurpose.LiveChat.ToString(),
            workspaceId,
            systemPrompt: "System prompt",
            persistChatSession: false);
    }
}

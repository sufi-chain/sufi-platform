using NSubstitute;
using Shouldly;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiPlatform.HelpDesk.Ai;
using SufiChain.SufiPlatform.HelpDesk.Options;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
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
    private readonly IHooshvareDefinitionRepository _hooshvareDefinitionRepository;
    private readonly HooshvareBusinessLocalizationService _businessLocalization;
    private readonly IHooshvareWorkspaceResolver _hooshvareWorkspaceResolver;
    private readonly HelpDeskAiWorkspaceResolver _resolver;

    public HelpDeskAiWorkspaceResolverTests()
    {
        _assignmentRepository = Substitute.For<IProjectAiWorkspaceAssignmentRepository>();
        _cache = Substitute.For<IDistributedCache<HelpDeskProjectAiWorkspaceAssignmentCacheItem>>();
        _optionsProvider = Substitute.For<IHelpDeskOptionsProvider>();
        _currentTenant = Substitute.For<ICurrentTenant>();
        _hooshvareDefinitionRepository = Substitute.For<IHooshvareDefinitionRepository>();
        _businessLocalization = new HooshvareBusinessLocalizationService(Substitute.For<IStringLocalizerFactory>());
        _hooshvareWorkspaceResolver = Substitute.For<IHooshvareWorkspaceResolver>();

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
            _hooshvareDefinitionRepository,
            _businessLocalization,
            new HelpDeskAiBindingEnricher(
                _hooshvareWorkspaceResolver,
                NullLogger<HelpDeskAiBindingEnricher>.Instance),
            NullLogger<HelpDeskAiWorkspaceResolver>.Instance);
    }

    [Fact]
    public async Task Should_Return_Purpose_Assignment_First()
    {
        var projectId = Guid.NewGuid();
        var defaultHooshvareId = Guid.NewGuid();
        var contentHooshvareId = Guid.NewGuid();
        var contentWorkspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId)
            .Returns(
            [
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.Default,
                    defaultHooshvareId,
                    "Default Hooshvare"),
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.ContentEditing,
                    contentHooshvareId,
                    "Content Hooshvare")
            ]);
        _hooshvareDefinitionRepository.GetAsync(contentHooshvareId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateHooshvare(contentHooshvareId, "Content Hooshvare", contentWorkspaceId));
        _hooshvareWorkspaceResolver.ResolveAsync(contentWorkspaceId)
            .Returns(new HooshvareWorkspaceBinding
            {
                WorkspaceId = contentWorkspaceId,
                WorkspaceName = "content-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.Project);
        result.HooshvareId.ShouldBe(contentHooshvareId);
        result.HooshvareName.ShouldBe("Content Hooshvare");
        result.WorkspaceId.ShouldBe(contentWorkspaceId);
        result.WorkspaceName.ShouldBe("content-workspace");
    }

    [Fact]
    public async Task Should_Fallback_To_Project_Default_Assignment()
    {
        var projectId = Guid.NewGuid();
        var defaultHooshvareId = Guid.NewGuid();
        var defaultWorkspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId)
            .Returns(
            [
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.Default,
                    defaultHooshvareId,
                    "Default Hooshvare")
            ]);
        _hooshvareDefinitionRepository.GetAsync(defaultHooshvareId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateHooshvare(defaultHooshvareId, "Default Hooshvare", defaultWorkspaceId));
        _hooshvareWorkspaceResolver.ResolveAsync(defaultWorkspaceId)
            .Returns(new HooshvareWorkspaceBinding
            {
                WorkspaceId = defaultWorkspaceId,
                WorkspaceName = "default-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.ProjectDefault);
        result.HooshvareId.ShouldBe(defaultHooshvareId);
        result.HooshvareName.ShouldBe("Default Hooshvare");
        result.WorkspaceId.ShouldBe(defaultWorkspaceId);
        result.WorkspaceName.ShouldBe("default-workspace");
    }

    [Fact]
    public async Task Should_Fallback_To_Tenant_Default_Hooshvare_When_Project_Assignment_Is_Missing()
    {
        var projectId = Guid.NewGuid();
        var tenantHooshvareId = Guid.NewGuid();
        var tenantWorkspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId).Returns([]);
        _optionsProvider.GetAsync().Returns(new HelpDeskOptions { DefaultAiHooshvareId = tenantHooshvareId });
        _hooshvareDefinitionRepository.GetAsync(tenantHooshvareId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateHooshvare(tenantHooshvareId, "Tenant Hooshvare", tenantWorkspaceId));
        _hooshvareWorkspaceResolver.ResolveAsync(tenantWorkspaceId)
            .Returns(new HooshvareWorkspaceBinding
            {
                WorkspaceId = tenantWorkspaceId,
                WorkspaceName = "tenant-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.TenantDefault);
        result.HooshvareId.ShouldBe(tenantHooshvareId);
        result.HooshvareName.ShouldBe("Tenant Hooshvare");
        result.WorkspaceId.ShouldBe(tenantWorkspaceId);
        result.WorkspaceName.ShouldBe("tenant-workspace");
    }

    [Fact]
    public async Task Should_Return_MissingAssignment_When_No_Assignment_And_No_Default_Setting()
    {
        var projectId = Guid.NewGuid();
        _assignmentRepository.GetListByProjectAsync(projectId).Returns([]);
        _optionsProvider.GetAsync().Returns(new HelpDeskOptions { DefaultAiHooshvareId = null });

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.IsReady.ShouldBeFalse();
        result.Readiness.ShouldBe(HelpDeskAiBindingReadiness.MissingAssignment);
        result.Source.ShouldBe(HelpDeskAiBindingSource.Missing);
    }

    [Fact]
    public async Task Should_Return_MissingWorkspace_When_Hooshvare_Has_No_Workspace()
    {
        var projectId = Guid.NewGuid();
        var hooshvareId = Guid.NewGuid();
        var workspaceId = Guid.NewGuid();

        _assignmentRepository.GetListByProjectAsync(projectId)
            .Returns([
                new ProjectAiWorkspaceAssignment(
                    Guid.NewGuid(),
                    projectId,
                    HelpDeskAiWorkspacePurpose.ContentEditing,
                    hooshvareId,
                    "Content Hooshvare")
            ]);
        _hooshvareDefinitionRepository.GetAsync(hooshvareId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateHooshvare(hooshvareId, "Content Hooshvare", workspaceId));
        _hooshvareWorkspaceResolver.ResolveAsync(workspaceId, Arg.Any<CancellationToken>())
            .Returns(Task.FromException<HooshvareWorkspaceBinding>(
                new InvalidOperationException("Workspace not found")));

        var result = await _resolver.ResolveAsync(projectId, HelpDeskAiWorkspacePurpose.ContentEditing);

        result.Readiness.ShouldBe(HelpDeskAiBindingReadiness.MissingWorkspace);
        result.HooshvareId.ShouldBe(hooshvareId);
        result.HooshvareName.ShouldBe("Content Hooshvare");
    }

    [Fact]
    public async Task Should_Use_Tenant_Default_When_ProjectId_Is_Null()
    {
        var tenantHooshvareId = Guid.NewGuid();
        var tenantWorkspaceId = Guid.NewGuid();
        _optionsProvider.GetAsync().Returns(new HelpDeskOptions { DefaultAiHooshvareId = tenantHooshvareId });
        _hooshvareDefinitionRepository.GetAsync(tenantHooshvareId, cancellationToken: Arg.Any<CancellationToken>())
            .Returns(CreateHooshvare(tenantHooshvareId, "Tenant Hooshvare", tenantWorkspaceId));
        _hooshvareWorkspaceResolver.ResolveAsync(tenantWorkspaceId)
            .Returns(new HooshvareWorkspaceBinding
            {
                WorkspaceId = tenantWorkspaceId,
                WorkspaceName = "tenant-workspace",
                IsReady = true
            });

        var result = await _resolver.ResolveAsync(null, HelpDeskAiWorkspacePurpose.LiveChat);

        result.IsReady.ShouldBeTrue();
        result.Source.ShouldBe(HelpDeskAiBindingSource.TenantDefault);
        result.HooshvareName.ShouldBe("Tenant Hooshvare");
        result.WorkspaceId.ShouldBe(tenantWorkspaceId);
        result.WorkspaceName.ShouldBe("tenant-workspace");
        await _assignmentRepository.DidNotReceiveWithAnyArgs().GetListByProjectAsync(default);
    }

    private static HooshvareDefinition CreateHooshvare(Guid id, string displayName, Guid workspaceId)
    {
        return new HooshvareDefinition(
            id,
            tenantId: null,
            sourceModule: "HelpDesk.LiveChat",
            displayName,
            HooshvareKind.Hooshvare,
            HelpDeskAiWorkspacePurpose.LiveChat.ToString(),
            workspaceId,
            systemPrompt: "System prompt",
            persistChatSession: false);
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class InheritedWorkspaceLiveViewTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IAIAppService _aiAppService;
    private readonly IAIModelCatalogAppService _catalog;
    private readonly IAIModelRouteResolver _routeResolver;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IInheritedWorkspaceProjectionSynchronizer _synchronizer;
    private readonly ICurrentTenant _currentTenant;

    public InheritedWorkspaceLiveViewTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _aiAppService = GetRequiredService<IAIAppService>();
        _catalog = GetRequiredService<IAIModelCatalogAppService>();
        _routeResolver = GetRequiredService<IAIModelRouteResolver>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _assignmentRepository = GetRequiredService<IWorkspaceAssignmentRepository>();
        _synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Inherited_workspace_reads_host_models_live_and_refuses_edits()
    {
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var host = await InsertHostWorkspaceAsync();

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(() => _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, host.WorkspaceId, projectionId),
                autoSave: true));

            var source = await _workspaceRepository.GetAsync(host.WorkspaceId, includeDetails: true);
            await _synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                host.WorkspaceId,
                source.Name);
        }

        using (_currentTenant.Change(tenantId))
        {
            var copied = (await _aiAppService.GetModelConfigurationsAsync(projectionId))
                .Single(item => item.ModelId == "openrouter/free");
            copied.Id.ShouldBe(host.ChatModelId);
            copied.IsUserSelectable.ShouldBeTrue();
            copied.AcceptsImageInput.ShouldBe(true);
            copied.AcceptsFileInput.ShouldBe(false);
            copied.SupportsReasoning.ShouldBe(true);
            copied.ReasoningEfforts.ShouldBe("low,high");
            copied.DefaultReasoningEffort.ShouldBe("low");
            copied.CapabilitySource.ShouldBe(ModelCapabilitySource.LiveCatalog);

            var routes = await _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = projectionId,
                CapabilityType = AICapabilityType.ChatCompletion
            });
            routes.ShouldContain(route => route.Id == host.ChatModelId && route.IsUserSelectable && route.AcceptsImageInput == true);
        }

        Guid addedModelId;
        using (_currentTenant.Change(null))
        {
            addedModelId = await AddHostChatModelAsync(host.WorkspaceId, "nvidia/nemotron-3.5-lightning:free");
        }

        using (_currentTenant.Change(tenantId))
        {
            var models = await _aiAppService.GetModelConfigurationsAsync(projectionId);
            var added = models.Single(item => item.ModelId == "nvidia/nemotron-3.5-lightning:free");
            added.Id.ShouldBe(addedModelId);
            added.IsUserSelectable.ShouldBeTrue();
            added.AcceptsImageInput.ShouldBe(false);
            added.SupportsReasoning.ShouldBe(true);
            added.CapabilitySource.ShouldBe(ModelCapabilitySource.Manual);

            var routes = await _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = projectionId,
                CapabilityType = AICapabilityType.ChatCompletion
            });
            routes.Count(route => route.IsUserSelectable).ShouldBeGreaterThanOrEqualTo(2);
            routes.ShouldContain(route => route.Id == addedModelId && route.IsUserSelectable);

            var resolved = await _routeResolver.ResolveAsync(
                projectionId,
                AICapabilityType.ChatCompletion,
                new AIModelRouteSelection { ModelConfigurationId = addedModelId });
            resolved.ModelId.ShouldBe("nvidia/nemotron-3.5-lightning:free");
            resolved.ModelConfiguration!.IsUserSelectable.ShouldBeTrue();
            resolved.ModelConfiguration.SupportsReasoning.ShouldBe(true);

            var updateWorkspace = await Should.ThrowAsync<BusinessException>(() =>
                _workspaceAppService.UpdateAsync(projectionId, new UpdateWorkspaceDto
                {
                    Name = "renamed-inherited",
                    Provider = AIProviderType.OpenRouter,
                    Model = "openrouter/free",
                    IsActive = true
                }));
            updateWorkspace.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceReadOnly);

            var deleteWorkspace = await Should.ThrowAsync<BusinessException>(() =>
                _workspaceAppService.DeleteAsync(projectionId));
            deleteWorkspace.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceReadOnly);

            var updateModel = await Should.ThrowAsync<BusinessException>(() =>
                _aiAppService.UpdateModelConfigurationAsync(addedModelId, new UpdateAIModelConfigurationDto
                {
                    ModelId = "changed-on-tenant",
                    IsUserSelectable = false
                }));
            updateModel.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceReadOnly);

            var deleteModel = await Should.ThrowAsync<BusinessException>(() =>
                _aiAppService.DeleteModelConfigurationAsync(host.ChatModelId));
            deleteModel.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceReadOnly);
        }

        using (_currentTenant.Change(null))
        {
            await _workspaceAppService.DeactivateAssignmentAsync(assignmentId);
        }

        using (_currentTenant.Change(tenantId))
        {
            var projection = await _workspaceAppService.GetAsync(projectionId);
            projection.IsInherited.ShouldBeTrue();
            projection.IsActive.ShouldBeFalse();

            var stillThere = await _aiAppService.GetModelConfigurationsAsync(projectionId);
            stillThere.ShouldContain(item => item.ModelId == "nvidia/nemotron-3.5-lightning:free");
        }

        using (_currentTenant.Change(null))
        {
            var source = await _workspaceRepository.GetAsync(host.WorkspaceId, includeDetails: true);
            source.ModelConfigurations.ShouldContain(item => item.Id == addedModelId);
        }
    }

    private async Task<(Guid WorkspaceId, Guid ChatModelId)> InsertHostWorkspaceAsync()
    {
        var workspaceId = Guid.NewGuid();
        Guid chatModelId = Guid.Empty;
        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workspace = new Workspace(
                    workspaceId,
                    "sufi-infrastructure-" + workspaceId.ToString("N")[..8],
                    AIProviderType.OpenRouter,
                    "openrouter/free");
                workspace.UpdateConfiguration(
                    "openrouter/free",
                    "sk-host",
                    "https://or-gateway.sufichain.com/v1",
                    inputCostPer1MTokens: 0m,
                    outputCostPer1MTokens: 0m);
                var chat = workspace.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "openrouter/free",
                    apiEndpoint: "https://or-gateway.sufichain.com/v1",
                    priority: 0,
                    displayName: "Free",
                    isUserSelectable: true);
                chat.SetChatCapabilities(
                    acceptsImageInput: true,
                    acceptsFileInput: false,
                    supportsReasoning: true,
                    reasoningEfforts: "low,high",
                    defaultReasoningEffort: "low",
                    capabilitySource: ModelCapabilitySource.LiveCatalog);
                chatModelId = chat.Id;
                await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            });
        }

        return (workspaceId, chatModelId);
    }

    private async Task<Guid> AddHostChatModelAsync(Guid workspaceId, string modelId)
    {
        Guid addedId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
            var added = workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                modelId,
                apiEndpoint: "https://or-gateway.sufichain.com/v1",
                priority: 10,
                displayName: modelId,
                isUserSelectable: true);
            added.SetChatCapabilities(
                acceptsImageInput: false,
                acceptsFileInput: false,
                supportsReasoning: true,
                reasoningEfforts: "low,medium",
                defaultReasoningEffort: "low",
                capabilitySource: ModelCapabilitySource.Manual);
            addedId = added.Id;
            await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
        });

        return addedId;
    }
}

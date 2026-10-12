using System;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

/// <summary>
/// Guards the Model Configurations page load path: unknown workspace → not found;
/// a host-scope workspace returns its routes. bUnit is skipped — the UI Blazor test
/// project does not reference AI Blazor without adding a new project reference.
/// </summary>
public class ModelConfigurationsPageLoadTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IAIAppService _aiAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentTenant _currentTenant;

    public ModelConfigurationsPageLoadTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _aiAppService = GetRequiredService<IAIAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Unknown_workspace_id_throws_entity_not_found()
    {
        var missingId = Guid.NewGuid();

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _workspaceAppService.GetAsync(missingId));

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            _aiAppService.GetModelConfigurationsAsync(missingId));
    }

    [Fact]
    public async Task Host_scope_workspace_returns_its_routes()
    {
        Guid workspaceId;
        var chatRouteId = Guid.Empty;

        using (_currentTenant.Change(null))
        {
            workspaceId = Guid.NewGuid();
            await WithUnitOfWorkAsync(async () =>
            {
                var workspace = new Workspace(
                    workspaceId,
                    "model-config-host-" + workspaceId.ToString("N")[..8],
                    AIProviderType.OpenAI,
                    "gpt-4o-mini");
                workspace.UpdateConfiguration(
                    "gpt-4o-mini",
                    "sk-host",
                    "https://api.openai.com/v1");
                var chat = workspace.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "gpt-4o-mini",
                    apiEndpoint: "https://api.openai.com/v1",
                    priority: 0,
                    displayName: "Chat",
                    isUserSelectable: true);
                chatRouteId = chat.Id;
                workspace.AddModelConfiguration(
                    AICapabilityType.Embeddings,
                    "text-embedding-3-small",
                    apiEndpoint: "https://api.openai.com/v1",
                    priority: 0,
                    displayName: "Embeddings");
                await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            });

            var dto = await _workspaceAppService.GetAsync(workspaceId);
            dto.Id.ShouldBe(workspaceId);
            dto.Name.ShouldStartWith("model-config-host-");

            var routes = await _aiAppService.GetModelConfigurationsAsync(workspaceId);
            routes.Count.ShouldBeGreaterThanOrEqualTo(2);
            routes.ShouldContain(route =>
                route.Id == chatRouteId
                && route.CapabilityType == AICapabilityType.ChatCompletion
                && route.ModelId == "gpt-4o-mini");
            routes.ShouldContain(route =>
                route.CapabilityType == AICapabilityType.Embeddings
                && route.ModelId == "text-embedding-3-small");
        }
    }
}

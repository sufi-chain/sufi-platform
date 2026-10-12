using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class WorkspaceDecisionsModelTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private const string BaseUrl = "https://or-gateway.sufichain.com/v1";

    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IAIAppService _aiAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentTenant _currentTenant;

    public WorkspaceDecisionsModelTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _aiAppService = GetRequiredService<IAIAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task A_saved_decisions_model_survives_a_reload()
    {
        var workspaceId = await InsertOpenRouterWorkspaceAsync("decisions-respan");

        await SaveDecisionsAsync(workspaceId, "decisions-respan", "respan/span-01-lite:free");

        var reloaded = await _workspaceAppService.GetAsync(workspaceId);
        reloaded.DecisionsModelId.ShouldBe("respan/span-01-lite:free");

        var decisionsRoutes = await ReadDecisionsRoutesAsync(workspaceId);
        decisionsRoutes.Count(route => route.IsEnabled).ShouldBe(1);
        decisionsRoutes.Single(route => route.IsEnabled).ModelId.ShouldBe("respan/span-01-lite:free");
    }

    [Fact]
    public async Task A_saved_decisions_model_keeps_its_vendor_specific_id()
    {
        var workspaceId = await InsertOpenRouterWorkspaceAsync("decisions-typesafe");

        await SaveDecisionsAsync(workspaceId, "decisions-typesafe", "typesafe/jev-1.13");

        var reloaded = await _workspaceAppService.GetAsync(workspaceId);
        reloaded.DecisionsModelId.ShouldBe("typesafe/jev-1.13");
    }

    [Fact]
    public async Task An_empty_field_disables_the_decisions_route_without_deleting_it()
    {
        var workspaceId = await InsertOpenRouterWorkspaceAsync("decisions-cleared");
        await SaveDecisionsAsync(workspaceId, "decisions-cleared", "respan/span-01-lite:free");

        await SaveDecisionsAsync(workspaceId, "decisions-cleared", null);

        var reloaded = await _workspaceAppService.GetAsync(workspaceId);
        reloaded.DecisionsModelId.ShouldBeNull();

        var decisionsRoutes = await ReadDecisionsRoutesAsync(workspaceId);
        decisionsRoutes.ShouldNotBeEmpty();
        decisionsRoutes.ShouldAllBe(route => !route.IsEnabled);
    }

    [Fact]
    public async Task Setting_the_decisions_model_again_re_enables_the_same_route()
    {
        var workspaceId = await InsertOpenRouterWorkspaceAsync("decisions-reenable");
        await SaveDecisionsAsync(workspaceId, "decisions-reenable", "respan/span-01-lite:free");
        var routeId = (await ReadDecisionsRoutesAsync(workspaceId)).Single().Id;

        await SaveDecisionsAsync(workspaceId, "decisions-reenable", null);
        await SaveDecisionsAsync(workspaceId, "decisions-reenable", "respan/span-01-lite:free");

        var reloaded = await _workspaceAppService.GetAsync(workspaceId);
        reloaded.DecisionsModelId.ShouldBe("respan/span-01-lite:free");

        var decisionsRoutes = await ReadDecisionsRoutesAsync(workspaceId);
        var enabled = decisionsRoutes.Single(route => route.IsEnabled);
        enabled.Id.ShouldBe(routeId);
        decisionsRoutes.Count.ShouldBe(1);
    }

    [Fact]
    public async Task A_saved_decisions_model_survives_a_reload_in_tenant_scope()
    {
        var tenantId = Guid.NewGuid();
        Guid workspaceId;

        using (_currentTenant.Change(tenantId))
        {
            workspaceId = await InsertOpenRouterWorkspaceAsync("decisions-tenant", tenantId);
            await SaveDecisionsAsync(workspaceId, "decisions-tenant", "respan/span-01-lite:free");

            var reloaded = await _workspaceAppService.GetAsync(workspaceId);
            reloaded.DecisionsModelId.ShouldBe("respan/span-01-lite:free");
        }
    }

    [Fact]
    public async Task The_model_configurations_list_includes_the_saved_decisions_route()
    {
        var workspaceId = await InsertOpenRouterWorkspaceAsync("decisions-listed");
        await SaveDecisionsAsync(workspaceId, "decisions-listed", "respan/span-01-lite:free");

        var configurations = await _aiAppService.GetModelConfigurationsAsync(workspaceId);
        var decisions = configurations.Single(route => route.CapabilityType == AICapabilityType.Decisions);

        decisions.ModelId.ShouldBe("respan/span-01-lite:free");
        decisions.IsEnabled.ShouldBeTrue();
    }

    [Fact]
    public async Task An_openai_compatible_provider_creates_no_decisions_route()
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                "decisions-incompatible",
                AIProviderType.OpenAICompatible,
                "chat-model");
            workspace.UpdateConfiguration("chat-model", "sk-workspace", "https://incompatible.example/v1");
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
        });

        await _workspaceAppService.UpdateAsync(workspaceId, new UpdateWorkspaceDto
        {
            Name = "decisions-incompatible",
            Provider = AIProviderType.OpenAICompatible,
            Model = "chat-model",
            ApiBaseUrl = "https://incompatible.example/v1",
            DecisionsModelId = "typesafe/jev-1.13",
            IsActive = true
        });

        (await ReadDecisionsRoutesAsync(workspaceId)).ShouldBeEmpty();
    }

    private async Task<Guid> InsertOpenRouterWorkspaceAsync(string name, Guid? tenantId = null)
    {
        var workspaceId = Guid.NewGuid();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                workspaceId,
                name,
                AIProviderType.OpenRouter,
                "openrouter/free",
                tenantId);
            workspace.UpdateConfiguration("openrouter/free", "sk-workspace", BaseUrl);
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
        });
        return workspaceId;
    }

    private Task SaveDecisionsAsync(Guid workspaceId, string name, string? decisionsModelId)
    {
        return _workspaceAppService.UpdateAsync(workspaceId, new UpdateWorkspaceDto
        {
            Name = name,
            Provider = AIProviderType.OpenRouter,
            Model = "openrouter/free",
            ApiBaseUrl = BaseUrl,
            DecisionsModelId = decisionsModelId,
            IsActive = true
        });
    }

    private async Task<System.Collections.Generic.List<AIModelConfiguration>> ReadDecisionsRoutesAsync(Guid workspaceId)
    {
        var routes = new System.Collections.Generic.List<AIModelConfiguration>();
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
            routes.AddRange(workspace.ModelConfigurations
                .Where(route => route.CapabilityType == AICapabilityType.Decisions));
        });
        return routes;
    }
}

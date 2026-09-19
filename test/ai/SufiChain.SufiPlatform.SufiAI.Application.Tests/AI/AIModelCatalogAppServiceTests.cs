using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.AI;

public class AIModelCatalogAppServiceTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IAIModelCatalogAppService _catalog;
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly ICurrentTenant _currentTenant;

    public AIModelCatalogAppServiceTests()
    {
        _catalog = GetRequiredService<IAIModelCatalogAppService>();
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public void Selectable_route_dto_must_omit_endpoint_and_credentials()
    {
        var names = typeof(AIModelRouteDto).GetProperties().Select(property => property.Name).ToHashSet();

        names.ShouldNotContain("ApiKey");
        names.ShouldNotContain("ApiEndpoint");
        names.ShouldNotContain("ApiBaseUrl");
        names.ShouldNotContain("Endpoint");
        names.ShouldContain("Id");
        names.ShouldContain("ModelId");
        names.ShouldContain("DisplayName");
        names.ShouldContain("OpenAIApiMode");
        names.ShouldContain("IsReady");
    }

    [Fact]
    public async Task Should_List_Selectable_Routes_Without_Endpoint_Or_Credentials()
    {
        var workspace = await InsertWorkspaceWithSelectableRoutesAsync("catalog-host");

        var routes = await _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
        {
            WorkspaceId = workspace.Id
        });

        routes.Count.ShouldBeGreaterThanOrEqualTo(2);
        routes.ShouldAllBe(route => route.Id != Guid.Empty);
        routes.ShouldContain(route => route.ModelId == "fast-chat");
        routes.ShouldContain(route => route.ModelId == "smart-chat");
    }

    [Fact]
    public async Task Should_Intersect_Hooshvare_Allowlist()
    {
        var workspace = await InsertWorkspaceWithSelectableRoutesAsync("catalog-allowlist");
        var allowed = workspace.ModelConfigurations.Single(route => route.ModelId == "fast-chat");
        var hooshvareId = Guid.NewGuid();
        TestHooshvarePolicy.Current.Value = new AIHooshvareModelSelectionPolicy
        {
            HooshvareId = hooshvareId,
            WorkspaceId = workspace.Id,
            AllowedModelConfigurationIds = new[] { allowed.Id }
        };

        try
        {
            var routes = await _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = workspace.Id,
                HooshvareId = hooshvareId
            });

            routes.Count.ShouldBe(1);
            routes[0].Id.ShouldBe(allowed.Id);
            routes[0].ModelId.ShouldBe("fast-chat");
        }
        finally
        {
            TestHooshvarePolicy.Current.Value = null;
        }
    }

    [Fact]
    public async Task Should_Hide_Tenant_Workspace_From_Another_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        Guid workspaceId;

        using (_currentTenant.Change(tenantA))
        {
            var workspace = await InsertWorkspaceWithSelectableRoutesAsync("catalog-tenant-a", tenantA);
            workspaceId = workspace.Id;

            var routes = await _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = workspaceId
            });
            routes.ShouldNotBeEmpty();
        }

        using (_currentTenant.Change(tenantB))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() =>
                _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
                {
                    WorkspaceId = workspaceId
                }));
        }
    }

    [Fact]
    public async Task Inherited_workspace_catalog_is_readable_and_mutations_are_rejected()
    {
        var tenantId = Guid.NewGuid();
        Guid workspaceId;

        using (_currentTenant.Change(tenantId))
        {
            var workspace = await InsertWorkspaceWithSelectableRoutesAsync("catalog-inherited", tenantId);
            workspace.MarkAsInherited(Guid.NewGuid(), Guid.NewGuid());
            await WithUnitOfWorkAsync(() => _workspaceRepository.UpdateAsync(workspace, autoSave: true));
            workspaceId = workspace.Id;

            var routes = await _catalog.GetSelectableRoutesAsync(new GetSelectableModelRoutesInput
            {
                WorkspaceId = workspaceId
            });
            routes.ShouldNotBeEmpty();

            var updateException = await Should.ThrowAsync<BusinessException>(() =>
                _workspaceAppService.UpdateAsync(workspaceId, new UpdateWorkspaceDto
                {
                    Name = "catalog-inherited",
                    Provider = AIProviderType.OpenAI,
                    Model = "fast-chat",
                    IsActive = true
                }));
            updateException.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceReadOnly);
        }
    }

    private async Task<Workspace> InsertWorkspaceWithSelectableRoutesAsync(string name, Guid? tenantId = null)
    {
        Guid workspaceId = Guid.Empty;
        await WithUnitOfWorkAsync(async () =>
        {
            var workspace = new Workspace(
                Guid.NewGuid(),
                name,
                AIProviderType.OpenAI,
                "workspace-default",
                tenantId);
            workspace.UpdateConfiguration(
                "workspace-default",
                "sk-workspace",
                "https://api.example/v1",
                inputCostPer1MTokens: 1m,
                outputCostPer1MTokens: 2m);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "fast-chat",
                apiEndpoint: "https://api.example/v1",
                apiKey: "sk-fast",
                priority: 0,
                displayName: "Fast",
                isUserSelectable: true,
                maxContextTokens: 8000);
            workspace.AddModelConfiguration(
                AICapabilityType.ChatCompletion,
                "smart-chat",
                apiEndpoint: "https://api.example/v1",
                apiKey: "sk-smart",
                priority: 10,
                displayName: "Smart",
                isUserSelectable: true,
                maxContextTokens: 128000);
            await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            workspaceId = workspace.Id;
        });

        return await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
    }
}

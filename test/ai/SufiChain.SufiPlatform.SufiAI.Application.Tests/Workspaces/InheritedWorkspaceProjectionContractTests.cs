using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.EntityFrameworkCore;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class InheritedWorkspaceProjectionContractTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IInheritedWorkspaceProjectionSynchronizer _synchronizer;
    private readonly IInheritedWorkspaceLiveView _liveView;
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly ICurrentTenant _currentTenant;

    public InheritedWorkspaceProjectionContractTests()
    {
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _assignmentRepository = GetRequiredService<IWorkspaceAssignmentRepository>();
        _synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        _liveView = GetRequiredService<IInheritedWorkspaceLiveView>();
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Projection_does_not_store_host_api_keys()
    {
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var hostWorkspaceId = Guid.NewGuid();

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workspace = new Workspace(
                    hostWorkspaceId,
                    "host-keys-" + hostWorkspaceId.ToString("N")[..8],
                    AIProviderType.OpenRouter,
                    "openrouter/free");
                workspace.UpdateConfiguration(
                    "openrouter/free",
                    "sk-host",
                    "https://or-gateway.sufichain.com/v1",
                    inputCostPer1MTokens: 1m,
                    outputCostPer1MTokens: 2m);
                workspace.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "openrouter/free",
                    apiEndpoint: "https://or-gateway.sufichain.com/v1",
                    apiKey: "sk-route",
                    priority: 0,
                    displayName: "Free",
                    isUserSelectable: true);
                await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            });

            await WithUnitOfWorkAsync(() => _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, hostWorkspaceId, projectionId),
                autoSave: true));

            var source = await _workspaceRepository.GetAsync(hostWorkspaceId, includeDetails: true);
            await _synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                hostWorkspaceId,
                source.Name);
        }

        using (_currentTenant.Change(tenantId))
        using (_liveView.Suppress())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var stored = await _workspaceRepository.GetAsync(projectionId, includeDetails: true);
                stored.TenantId.ShouldBe(tenantId);
                stored.IsInherited.ShouldBeTrue();
                stored.ApiKey.ShouldBeNull();
                stored.ModelConfigurations.ShouldNotBeEmpty();
                stored.ModelConfigurations.ShouldAllBe(configuration => configuration.ApiKey == null);
            });
        }

        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var live = await _workspaceRepository.GetAsync(projectionId, includeDetails: true);
                live.ApiKey.ShouldBe("sk-host");
                live.ModelConfigurations.Single(item => item.ModelId == "openrouter/free").ApiKey.ShouldBe("sk-route");
            });
        }

        using (_currentTenant.Change(null))
        using (_liveView.Suppress())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = await _workspaceRepository.GetAsync(hostWorkspaceId, includeDetails: true);
                host.ApiKey.ShouldBe("sk-host");
                host.ModelConfigurations.Single().ApiKey.ShouldBe("sk-route");
            });
        }
    }

    [Fact]
    public async Task Unassign_deletes_the_projected_workspace_and_its_routes()
    {
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var hostWorkspaceId = Guid.NewGuid();

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workspace = new Workspace(
                    hostWorkspaceId,
                    "host-unassign-" + hostWorkspaceId.ToString("N")[..8],
                    AIProviderType.OpenRouter,
                    "openrouter/free");
                workspace.UpdateConfiguration(
                    "openrouter/free",
                    "sk-host",
                    "https://or-gateway.sufichain.com/v1");
                workspace.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "openrouter/free",
                    apiKey: "sk-route",
                    priority: 0);
                workspace.SetGuardrail(WorkspaceGuardrailPeriod.Daily, 3m);
                await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            });

            await WithUnitOfWorkAsync(() => _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, hostWorkspaceId, projectionId),
                autoSave: true));

            var source = await _workspaceRepository.GetAsync(hostWorkspaceId, includeDetails: true);
            await _synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                hostWorkspaceId,
                source.Name);

            await _workspaceAppService.DeactivateAssignmentAsync(assignmentId);
        }

        using (_currentTenant.Change(tenantId))
        {
            await Should.ThrowAsync<EntityNotFoundException>(() => _workspaceAppService.GetAsync(projectionId));
        }

        await WithUnitOfWorkAsync(async () =>
        {
            var db = (DbContext)await GetRequiredService<IDbContextProvider<IAIDbContext>>().GetDbContextAsync();
            (await db.Set<Workspace>().IgnoreQueryFilters().CountAsync(workspace => workspace.Id == projectionId)).ShouldBe(0);
            (await db.Set<AIModelConfiguration>().IgnoreQueryFilters().CountAsync(route => route.WorkspaceId == projectionId)).ShouldBe(0);
            (await db.Set<WorkspaceGuardrail>().IgnoreQueryFilters().CountAsync(guardrail => guardrail.WorkspaceId == projectionId)).ShouldBe(0);
        });

        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            var assignment = await WithUnitOfWorkAsync(() => _assignmentRepository.GetAsync(assignmentId));
            assignment.IsActive.ShouldBeFalse();
        }

        using (_currentTenant.Change(null))
        using (_liveView.Suppress())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var host = await _workspaceRepository.GetAsync(hostWorkspaceId, includeDetails: true);
                host.ApiKey.ShouldBe("sk-host");
                host.ModelConfigurations.ShouldNotBeEmpty();
                host.Guardrails.ShouldContain(guardrail => guardrail.Period == WorkspaceGuardrailPeriod.Daily);
            });
        }
    }
}

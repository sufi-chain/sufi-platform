using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

public class InheritedWorkspaceShellTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IInheritedWorkspaceProjectionSynchronizer _synchronizer;
    private readonly IInheritedWorkspaceLiveView _liveView;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;

    public InheritedWorkspaceShellTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _assignmentRepository = GetRequiredService<IWorkspaceAssignmentRepository>();
        _synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        _liveView = GetRequiredService<IInheritedWorkspaceLiveView>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
        _dataFilter = GetRequiredService<IDataFilter>();
    }

    [Fact]
    public async Task Projection_keeps_the_host_name_and_stores_no_host_keys()
    {
        var (tenantId, assignmentId, projectionId, source) = await AssignHostWorkspaceAsync();

        using (_currentTenant.Change(tenantId))
        using (_liveView.Suppress())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var shell = await _workspaceRepository.GetAsync(projectionId, includeDetails: true);
                shell.Name.ShouldBe(source.Name);
                shell.IsInherited.ShouldBeTrue();
                shell.SourceWorkspaceId.ShouldBe(source.Id);
                shell.AssignmentId.ShouldBe(assignmentId);
                shell.ApiKey.ShouldBeNull();
                shell.ApiBaseUrl.ShouldBeNull();
                shell.ModelConfigurations.ShouldBeEmpty();
                shell.Guardrails.ShouldBeEmpty();
            });
        }
    }

    [Fact]
    public async Task Projection_reports_a_name_clash_instead_of_adding_a_suffix()
    {
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var source = await InsertHostWorkspaceAsync("shared-" + Guid.NewGuid().ToString("N")[..8]);

        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                await _workspaceRepository.InsertAsync(
                    new Workspace(Guid.NewGuid(), source.Name, AIProviderType.OpenAI, "gpt-4o-mini", tenantId),
                    autoSave: true);
            });
        }

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(() => _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, source.Id, projectionId),
                autoSave: true));

            var error = await Should.ThrowAsync<Volo.Abp.BusinessException>(() =>
                _synchronizer.CreateTenantProjectionAsync(
                    source,
                    tenantId,
                    assignmentId,
                    projectionId,
                    source.Id,
                    source.Name));
            error.Code.ShouldBe(AIErrorCodes.WorkspaceNameAlreadyExists);
        }

        using (_currentTenant.Change(tenantId))
        using (_liveView.Suppress())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workspaces = await _workspaceRepository.GetListAsync(maxResultCount: int.MaxValue);
                workspaces.ShouldNotContain(workspace => workspace.Name.StartsWith(source.Name + "-", StringComparison.Ordinal));
                workspaces.Count(workspace => workspace.Name == source.Name).ShouldBe(1);
                (await _workspaceRepository.FindAsync(projectionId)).ShouldBeNull();
            });
        }
    }

    [Fact]
    public async Task Unassign_deletes_the_shell_and_keeps_the_assignment_inactive()
    {
        var (tenantId, assignmentId, projectionId, _) = await AssignHostWorkspaceAsync();

        using (_currentTenant.Change(null))
        {
            await _workspaceAppService.DeactivateAssignmentAsync(assignmentId);
        }

        using (_currentTenant.Change(tenantId))
        using (_dataFilter.Disable<ISoftDelete>())
        using (_liveView.Suppress())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var shell = await _workspaceRepository.FindAsync(projectionId);
                shell.ShouldBeNull();
            });
        }

        using (_dataFilter.Disable<IMultiTenant>())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var assignment = await _assignmentRepository.GetAsync(assignmentId);
                assignment.IsActive.ShouldBeFalse();
                assignment.TargetWorkspaceId.ShouldBe(projectionId);
            });
        }
    }

    private async Task<(Guid TenantId, Guid AssignmentId, Guid ProjectionId, Workspace Source)> AssignHostWorkspaceAsync()
    {
        var tenantId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var projectionId = Guid.NewGuid();
        var source = await InsertHostWorkspaceAsync("host-shell-" + Guid.NewGuid().ToString("N")[..8]);

        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(() => _assignmentRepository.InsertAsync(
                new WorkspaceAssignment(assignmentId, tenantId, source.Id, projectionId),
                autoSave: true));

            await _synchronizer.CreateTenantProjectionAsync(
                source,
                tenantId,
                assignmentId,
                projectionId,
                source.Id,
                source.Name);
        }

        return (tenantId, assignmentId, projectionId, source);
    }

    private async Task<Workspace> InsertHostWorkspaceAsync(string name)
    {
        var workspaceId = Guid.NewGuid();
        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var workspace = new Workspace(
                    workspaceId,
                    name,
                    AIProviderType.OpenRouter,
                    "openrouter/free");
                workspace.UpdateConfiguration(
                    "openrouter/free",
                    "sk-host",
                    "https://or-gateway.sufichain.com/v1",
                    inputCostPer1MTokens: 0m,
                    outputCostPer1MTokens: 0m);
                workspace.AddModelConfiguration(
                    AICapabilityType.ChatCompletion,
                    "openrouter/free",
                    apiEndpoint: "https://or-gateway.sufichain.com/v1",
                    apiKey: "sk-route",
                    priority: 0,
                    displayName: "Free",
                    isUserSelectable: true);
                workspace.SetGuardrail(WorkspaceGuardrailPeriod.Daily, 4m);
                await _workspaceRepository.InsertAsync(workspace, autoSave: true);
            });
        }

        Workspace? stored = null;
        using (_currentTenant.Change(null))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                stored = await _workspaceRepository.GetAsync(workspaceId, includeDetails: true);
            });
        }

        return stored!;
    }
}

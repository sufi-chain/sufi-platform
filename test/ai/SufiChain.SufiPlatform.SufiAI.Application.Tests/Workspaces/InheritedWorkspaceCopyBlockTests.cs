using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

// #499: a tenant must not copy an inherited workspace (and with it the host ApiKey and route keys)
// into an editable workspace of its own.
public class InheritedWorkspaceCopyBlockTests : SufiAITestBase<SufiAIApplicationTestModule>
{
    private readonly IWorkspaceAppService _workspaceAppService;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IInheritedWorkspaceProjectionSynchronizer _synchronizer;
    private readonly ICurrentTenant _currentTenant;

    public InheritedWorkspaceCopyBlockTests()
    {
        _workspaceAppService = GetRequiredService<IWorkspaceAppService>();
        _workspaceRepository = GetRequiredService<IWorkspaceRepository>();
        _assignmentRepository = GetRequiredService<IWorkspaceAssignmentRepository>();
        _synchronizer = GetRequiredService<IInheritedWorkspaceProjectionSynchronizer>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Tenant_cannot_clone_an_inherited_workspace()
    {
        var (tenantId, _, projectionId) = await AssignHostWorkspaceAsync();
        var cloneName = "copied-" + Guid.NewGuid().ToString("N")[..8];

        using (_currentTenant.Change(tenantId))
        {
            var error = await Should.ThrowAsync<BusinessException>(() =>
                _workspaceAppService.CloneAsync(projectionId, new CloneWorkspaceDto { Name = cloneName }));
            error.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceCopyBlocked);

            await WithUnitOfWorkAsync(async () =>
            {
                var workspaces = await _workspaceRepository.GetListAsync(includeDetails: false);
                workspaces.ShouldNotContain(workspace => workspace.Name == cloneName);
                workspaces.Where(workspace => !workspace.IsInherited)
                    .ShouldNotContain(workspace => workspace.ApiKey == "sk-host");
            });
        }
    }

    [Fact]
    public async Task Tenant_cannot_convert_an_inherited_workspace_to_custom()
    {
        var (tenantId, assignmentId, projectionId) = await AssignHostWorkspaceAsync();

        using (_currentTenant.Change(tenantId))
        {
            var error = await Should.ThrowAsync<BusinessException>(() =>
                _workspaceAppService.ConvertToCustomAsync(projectionId));
            error.Code.ShouldBe(AIErrorCodes.InheritedWorkspaceCopyBlocked);

            var projection = await _workspaceAppService.GetAsync(projectionId);
            projection.IsInherited.ShouldBeTrue();
        }

        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var assignment = await _assignmentRepository.GetAsync(assignmentId);
                assignment.IsActive.ShouldBeTrue();
            });
        }
    }

    [Fact]
    public async Task Tenant_can_still_clone_its_own_workspace()
    {
        var tenantId = Guid.NewGuid();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        using (_currentTenant.Change(tenantId))
        {
            var own = await _workspaceAppService.CreateAsync(new CreateWorkspaceDto
            {
                Name = "tenant-own-" + suffix,
                Provider = AIProviderType.OpenAI,
                Model = "gpt-4o-mini",
                ApiKey = "sk-tenant",
                ApiBaseUrl = "https://api.openai.com/v1"
            });

            var clone = await _workspaceAppService.CloneAsync(own.Id, new CloneWorkspaceDto { Name = "tenant-own-copy-" + suffix });

            clone.Id.ShouldNotBe(own.Id);
            clone.Name.ShouldBe("tenant-own-copy-" + suffix);
            clone.IsInherited.ShouldBeFalse();

            // The copy carries the tenant's own key, never a host key.
            await WithUnitOfWorkAsync(async () =>
            {
                var workspaces = await _workspaceRepository.GetListAsync(includeDetails: false);
                var copy = workspaces.Single(workspace => workspace.Name == "tenant-own-copy-" + suffix);
                copy.ApiKey.ShouldBe("sk-tenant");
                copy.ApiKey.ShouldNotBe("sk-host");
            });
        }
    }

    [Fact]
    public async Task Convert_to_custom_still_rejects_a_workspace_that_is_not_inherited()
    {
        var tenantId = Guid.NewGuid();

        using (_currentTenant.Change(tenantId))
        {
            var own = await _workspaceAppService.CreateAsync(new CreateWorkspaceDto
            {
                Name = "tenant-plain-" + Guid.NewGuid().ToString("N")[..8],
                Provider = AIProviderType.OpenAI,
                Model = "gpt-4o-mini",
                ApiKey = "sk-tenant",
                ApiBaseUrl = "https://api.openai.com/v1"
            });

            var error = await Should.ThrowAsync<BusinessException>(() =>
                _workspaceAppService.ConvertToCustomAsync(own.Id));
            error.Code.ShouldBe(AIErrorCodes.WorkspaceNotInherited);
        }
    }

    private async Task<(Guid TenantId, Guid AssignmentId, Guid ProjectionId)> AssignHostWorkspaceAsync()
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
                    "host-shared-" + hostWorkspaceId.ToString("N")[..8],
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

        return (tenantId, assignmentId, projectionId);
    }
}

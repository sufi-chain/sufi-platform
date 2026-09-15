using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public class InheritedWorkspaceProjectionSynchronizer :
    IInheritedWorkspaceProjectionSynchronizer,
    ITransientDependency
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly WorkspaceManager _workspaceManager;
    private readonly InheritedWorkspaceProjectionRequestState _requestState;

    public InheritedWorkspaceProjectionSynchronizer(
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        IUnitOfWorkManager unitOfWorkManager,
        IWorkspaceRepository workspaceRepository,
        IWorkspaceAssignmentRepository assignmentRepository,
        WorkspaceManager workspaceManager,
        InheritedWorkspaceProjectionRequestState requestState)
    {
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _unitOfWorkManager = unitOfWorkManager;
        _workspaceRepository = workspaceRepository;
        _assignmentRepository = assignmentRepository;
        _workspaceManager = workspaceManager;
        _requestState = requestState;
    }

    public virtual async Task EnsureCurrentTenantAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTenant.Id is not Guid tenantId)
        {
            return;
        }

        if (_requestState.EnsuredTenantId == tenantId)
        {
            return;
        }

        _requestState.EnsuredTenantId = tenantId;

        var assignments = await LoadHostAssignmentsAsync(tenantId, cancellationToken);
        foreach (var assignment in assignments)
        {
            if (!assignment.IsActive)
            {
                continue;
            }

            var local = await _workspaceRepository.FindAsync(
                assignment.TargetWorkspaceId,
                includeDetails: false,
                cancellationToken: cancellationToken);
            if (local != null)
            {
                continue;
            }

            var source = await LoadHostWorkspaceAsync(assignment, cancellationToken);
            if (source == null)
            {
                continue;
            }

            await CreateTenantProjectionAsync(
                source,
                tenantId,
                assignment.Id,
                assignment.TargetWorkspaceId,
                assignment.SourceWorkspaceId,
                source.Name,
                cancellationToken);
        }
    }

    public virtual async Task<Workspace> CreateTenantProjectionAsync(
        Workspace template,
        Guid tenantId,
        Guid assignmentId,
        Guid targetWorkspaceId,
        Guid sourceWorkspaceId,
        string? preferredName,
        CancellationToken cancellationToken = default)
    {
        using (_currentTenant.Change(tenantId))
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var existing = await _workspaceRepository.FindAsync(
                    targetWorkspaceId,
                    includeDetails: true,
                    cancellationToken: cancellationToken);
                if (existing != null)
                {
                    await uow.CompleteAsync();
                    return existing;
                }

                var name = await _workspaceManager.ResolveUniqueNameAsync(
                    string.IsNullOrWhiteSpace(preferredName) ? template.Name : preferredName.Trim());
                var copy = _workspaceManager.CreateCopy(template, name, tenantId, targetWorkspaceId);
                copy.MarkAsInherited(sourceWorkspaceId, assignmentId);
                await _workspaceRepository.InsertAsync(copy, autoSave: true, cancellationToken);
                await uow.CompleteAsync();
                return copy;
            }
        }
    }

    public virtual async Task DeactivateTenantProjectionAsync(
        Guid tenantId,
        Guid targetWorkspaceId,
        CancellationToken cancellationToken = default)
    {
        using (_currentTenant.Change(tenantId))
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            {
                var workspace = await _workspaceRepository.FindAsync(
                    targetWorkspaceId,
                    includeDetails: false,
                    cancellationToken: cancellationToken);
                if (workspace is { IsInherited: true, IsActive: true })
                {
                    workspace.Deactivate();
                    await _workspaceRepository.UpdateAsync(workspace, autoSave: true, cancellationToken);
                }

                await uow.CompleteAsync();
            }
        }
    }

    public virtual async Task DeactivateHostAssignmentAsync(
        Guid assignmentId,
        CancellationToken cancellationToken = default)
    {
        using (_currentTenant.Change(null))
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var assignment = await _assignmentRepository.FindAsync(assignmentId, cancellationToken: cancellationToken);
                if (assignment is { IsActive: true })
                {
                    assignment.Deactivate();
                    await _assignmentRepository.UpdateAsync(assignment, autoSave: true, cancellationToken);
                }

                await uow.CompleteAsync();
            }
        }
    }

    private async Task<System.Collections.Generic.List<WorkspaceAssignment>> LoadHostAssignmentsAsync(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        using (_currentTenant.Change(null))
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var assignments = await _assignmentRepository.GetActiveByTenantAsync(tenantId, cancellationToken);
                await uow.CompleteAsync();
                return assignments;
            }
        }
    }

    private async Task<Workspace?> LoadHostWorkspaceAsync(
        WorkspaceAssignment assignment,
        CancellationToken cancellationToken)
    {
        using (_currentTenant.Change(null))
        {
            using (var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            using (_dataFilter.Disable<IMultiTenant>())
            {
                var misplaced = await _workspaceRepository.FindAsync(
                    assignment.TargetWorkspaceId,
                    includeDetails: true,
                    cancellationToken: cancellationToken);
                if (misplaced != null)
                {
                    await uow.CompleteAsync();
                    return misplaced;
                }

                var source = await _workspaceRepository.FindAsync(
                    assignment.SourceWorkspaceId,
                    includeDetails: true,
                    cancellationToken: cancellationToken);
                await uow.CompleteAsync();
                return source;
            }
        }
    }
}

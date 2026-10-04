using SufiChain.SufiPlatform.SufiAI;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

/// <summary>
/// Blocks workspace deletion while an active assignment or an active inherited copy still points at it.
/// Model configurations and guardrails are owned by the workspace. Usage logs are history.
/// Soft-deleted rows are ignored. Inactive assignments and inactive copies are ignored.
/// </summary>
[ExposeServices(typeof(IAiWorkspaceUsageChecker))]
public class WorkspaceStructuralUsageChecker : IAiWorkspaceUsageChecker, ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IWorkspaceAssignmentRepository _assignmentRepository;
    private readonly IDataFilter _dataFilter;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public WorkspaceStructuralUsageChecker(
        IWorkspaceRepository workspaceRepository,
        IWorkspaceAssignmentRepository assignmentRepository,
        IDataFilter dataFilter,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _workspaceRepository = workspaceRepository;
        _assignmentRepository = assignmentRepository;
        _dataFilter = dataFilter;
        _asyncExecuter = asyncExecuter;
    }

    public virtual async Task<IReadOnlyList<AiWorkspaceUsage>> FindActiveUsagesAsync(
        Guid workspaceId,
        CancellationToken cancellationToken = default)
    {
        if (workspaceId == Guid.Empty)
        {
            return Array.Empty<AiWorkspaceUsage>();
        }

        using (_dataFilter.Disable<IMultiTenant>())
        {
            var assignmentCount = await _asyncExecuter.CountAsync(
                (await _assignmentRepository.GetQueryableAsync())
                    .Where(assignment =>
                        assignment.IsActive
                        && (assignment.SourceWorkspaceId == workspaceId
                            || assignment.TargetWorkspaceId == workspaceId)),
                cancellationToken);

            var inheritedCount = await _asyncExecuter.CountAsync(
                (await _workspaceRepository.GetQueryableAsync())
                    .Where(workspace => workspace.IsActive && workspace.SourceWorkspaceId == workspaceId),
                cancellationToken);

            var usages = new List<AiWorkspaceUsage>();
            if (assignmentCount > 0)
            {
                usages.Add(new AiWorkspaceUsage
                {
                    RelationKey = AiWorkspaceUsageRelations.Assignment,
                    ActiveCount = assignmentCount
                });
            }

            if (inheritedCount > 0)
            {
                usages.Add(new AiWorkspaceUsage
                {
                    RelationKey = AiWorkspaceUsageRelations.InheritedWorkspace,
                    ActiveCount = inheritedCount
                });
            }

            return usages;
        }
    }
}

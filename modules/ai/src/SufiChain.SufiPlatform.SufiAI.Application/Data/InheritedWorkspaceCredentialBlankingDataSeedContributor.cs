using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.SufiAI.Data;

/// <summary>
/// Clears copied host credentials on inherited projections in the current database scope.
/// A shared host database is also covered by <c>InheritedWorkspaceCredentialBlanking</c> SQL,
/// because host scope cannot see tenant rows. Separate tenant databases are seeded in tenant scope.
/// The contributor only writes a count. It never logs key material.
/// </summary>
public class InheritedWorkspaceCredentialBlankingDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IWorkspaceRepository _workspaceRepository;
    private readonly IInheritedWorkspaceLiveView _liveView;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;
    private readonly IUnitOfWorkManager _unitOfWorkManager;
    private readonly ILogger<InheritedWorkspaceCredentialBlankingDataSeedContributor> _logger;

    public InheritedWorkspaceCredentialBlankingDataSeedContributor(
        IWorkspaceRepository workspaceRepository,
        IInheritedWorkspaceLiveView liveView,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        IUnitOfWorkManager unitOfWorkManager,
        ILogger<InheritedWorkspaceCredentialBlankingDataSeedContributor> logger)
    {
        _workspaceRepository = workspaceRepository;
        _liveView = liveView;
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _unitOfWorkManager = unitOfWorkManager;
        _logger = logger;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (_currentTenant.Change(context?.TenantId))
        using (_dataFilter.Disable<ISoftDelete>())
        using (var unitOfWork = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: true))
        using (_liveView.Suppress())
        {
            var workspaces = await _workspaceRepository.ListInheritedStoredAsync();
            var cleared = 0;
            foreach (var workspace in workspaces)
            {
                if (string.IsNullOrEmpty(workspace.ApiKey) &&
                    workspace.ModelConfigurations.All(item => string.IsNullOrEmpty(item.ApiKey)))
                {
                    continue;
                }

                workspace.ClearStoredCredentials();
                await _workspaceRepository.UpdateAsync(workspace, autoSave: true);
                cleared++;
            }

            await unitOfWork.CompleteAsync();
            if (cleared > 0)
            {
                _logger.LogInformation(
                    "Cleared copied credentials on {Count} inherited AI workspaces.",
                    cleared);
            }
        }
    }
}

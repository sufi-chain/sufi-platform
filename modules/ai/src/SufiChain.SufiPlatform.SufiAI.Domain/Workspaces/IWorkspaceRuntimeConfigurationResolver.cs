using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using SufiChain.SufiPlatform.SufiAI;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public interface IWorkspaceRuntimeConfigurationResolver : ITransientDependency
{
    WorkspaceRuntimeConfiguration Resolve(
        Workspace workspace,
        AICapabilityType capabilityType,
        AIModelConfiguration? configuration = null,
        bool isExplicitSelection = false);

    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);

    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken = default);

    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);

    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        AIModelRouteSelection selection,
        CancellationToken cancellationToken = default);

    void EnsureReady(WorkspaceRuntimeConfiguration configuration, bool requiresToolCalling = false);
}

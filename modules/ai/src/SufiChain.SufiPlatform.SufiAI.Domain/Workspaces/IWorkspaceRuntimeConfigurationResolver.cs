using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Workspaces;

public interface IWorkspaceRuntimeConfigurationResolver : ITransientDependency
{
    WorkspaceRuntimeConfiguration Resolve(
        Workspace workspace,
        AICapabilityType capabilityType);

    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        string workspaceName,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);

    Task<WorkspaceRuntimeConfiguration> ResolveAsync(
        Guid workspaceId,
        AICapabilityType capabilityType,
        CancellationToken cancellationToken = default);
}
